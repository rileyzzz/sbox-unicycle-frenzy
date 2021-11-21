using System;
using System.Numerics;
using Sandbox;
using Sandbox.ScreenShake;

internal partial class UnicycleController : BasePlayerController
{

	[Net, Predicted]
	public float PedalPosition { get; set; } // -1 = left pedal down, 1 = right pedal down, 0 = flat
	[Net, Predicted]
	public Angles Lean { get; set; }
	[Net]
	public float PedalTime { get; set; } = .75f;
	[Net]
	public float PedalResetAfter { get; set; } = 1.5f;
	[Net]
	public float PedalResetTime { get; set; } = .75f;
	[Net]
	public float MinPedalStrength { get; set; } = 10f;
	[Net]
	public float MaxPedalStrength { get; set; } = 50f;
	[Net]
	public float JumpStrength { get; set; } = 300f;
	[Net]
	public float PerfectPedalMultiplier { get; set; } = 1.25f;
	[Net]
	public float MaxLean { get; set; } = 45f;
	[Net]
	public float LeanSpeed { get; set; } = 80f;
	[Net]
	public float TurnSpeed { get; set; } = 5f;
	[Net]
	public float SlopeSpeed { get; set; } = 1600f;


	private string groundSurface;
	private Unstuck unstuck;
	private float timeToReachTarget;
	private float pedalStartPosition;
	private float pedalTargetPosition;
	private TimeSince timeSincePedalStart;
	private bool prevGrounded;
	private Vector3 prevVelocity;
	private Rotation targetFwd;

	public Vector3 Mins => new( -8, -8, 0 );
	public Vector3 Maxs => new( 8, 8, 48 );

	public UnicycleController()
	{
		unstuck = new( this );
	}

	public override void FrameSimulate()
	{
		base.FrameSimulate();

		EyeRot = Rotation.Identity;

		if ( Debug )
		{
			DebugOverlay.Text( Position, "Speed: " + Velocity.Length );
			DebugOverlay.Text( Position + Vector3.Down * 3, "Grounded: " + (GroundEntity != null) );
			DebugOverlay.Text( Position + Vector3.Down * 6, "GroundNormal: " + GroundNormal );
			DebugOverlay.Text( Position + Vector3.Down * 9, "Surface: " + groundSurface );
			DebugOverlay.Text( Position + Vector3.Down * 12, "Water Level: " + Pawn.WaterLevel.Fraction );

			DebugOverlay.Line( Position, Position + Velocity, Color.Yellow );
		}
	}

	public override void Simulate()
	{
		if ( Pawn is not UnicyclePlayer pl ) return;
		if ( unstuck.TestAndFix() ) return;

		SetTag( "sitting" );
		TryPedal();
		Gravity();
		CheckJump();
		DoSlope();
		Friction();

		// lerp pedals into place, adding velocity and lean
		if ( timeSincePedalStart < timeToReachTarget + Time.Delta )
		{
			var a = timeSincePedalStart / timeToReachTarget;
			a = Easing.EaseOut( a );

			var newPosition = pedalStartPosition.LerpTo( pedalTargetPosition, a );
			var delta = newPosition - PedalPosition;

			MovePedals( delta );
		}

		// lean from input
		Lean += new Angles( Input.Forward, 0, -Input.Left ) * Time.Delta * LeanSpeed;

		// momentum helps keep us straight
		var recover = Math.Min( Velocity.WithZ( 0 ).Length / 125f, 1f );
		Lean = Angles.Lerp( Lean, Angles.Zero, recover * Time.Delta );

		// do rotation if we're in the air or moving a little bit
		var spd = Velocity.WithZ( 0 ).Length;
		var grounded = GroundEntity != null;
		if ( (!grounded && spd < 50) || (grounded && spd > 20) )
		{
			var inputFwd = Rotation.LookAt( Input.Rotation.Forward.WithZ( 0 ), Vector3.Up );
			targetFwd = Rotation.Slerp( targetFwd, inputFwd, Time.Delta * 2f );
		}
		else
		{
			targetFwd = Rotation;
		}

		var targetRot = FromToRotation( Vector3.Up, GroundNormal );
		targetRot *= targetFwd;
		targetRot *= Rotation.From( Lean );

		Rotation = Rotation.Slerp( Rotation, targetRot, Time.Delta * TurnSpeed );

		if ( GroundEntity != null && Velocity.Length > 10 )
			Velocity = ClipVelocity( Velocity, Rotation.Right );

		// go
		Move();
		CheckGround();

		if ( pl.IsServer && ShouldFall() )
			pl.Fall();

		prevGrounded = GroundEntity != null;
		prevVelocity = Velocity;
	}

	private bool ShouldFall()
	{
		var spd = Velocity.WithZ( 0 ).Length;

		if ( GroundEntity != null && !prevGrounded )
		{
			if ( prevVelocity.z < -1000 )
				return true;
		}

		//if ( GroundEntity != null && spd > 5 )
		//{
		//	var d = MathF.Abs( Vector3.Dot( Rotation.Forward, prevVelocity.WithZ( 0 ).Normal ) );
		//	if ( d < .8f )
		//		return true;
		//}

		var ang = Rotation.Angles();
		var aroll = Math.Abs( ang.roll );
		var apitch = Math.Abs( ang.pitch );
		var maxLean = GroundEntity != null ? MaxLean : 180;

		if ( aroll > maxLean || apitch > maxLean )
			return true;

		return false;
	}

	private void Move()
	{
		if ( Velocity.Length < 1.0f )
		{
			Velocity = Vector3.Zero;
			return;
		}

		var mover = new MoveHelper( Position, Velocity );
		mover.Trace = mover.Trace.Size( Mins, Maxs ).Ignore( Pawn );
		mover.MaxStandableAngle = 75f;
		mover.TryMove( Time.Delta );

		Position = mover.Position;
		Velocity = mover.Velocity;
	}

	private void StayOnGround()
	{
		var start = Position + Vector3.Up * 2;
		var end = Position + Vector3.Down * 16;

		// See how far up we can go without getting stuck
		var trace = TraceBBox( Position, start );
		start = trace.EndPos;

		// Now trace down from a known safe position
		trace = TraceBBox( start, end );

		if ( trace.Fraction <= 0 ) return;
		if ( trace.Fraction >= 1 ) return;
		if ( trace.StartedSolid ) return;
		//if ( Vector3.GetAngle( Vector3.Up, trace.Normal ) > GroundAngle ) return;

		// This is incredibly hacky. The real problem is that trace returning that strange value we can't network over.
		// float flDelta = fabs( mv->GetAbsOrigin().z - trace.m_vEndPos.z );
		// if ( flDelta > 0.5f * DIST_EPSILON )

		Position = trace.EndPos + Vector3.Up * 5;
	}

	private void DoSlope()
	{
		if ( GroundEntity == null ) return;

		var slopeAngle = Vector3.GetAngle( GroundNormal, Vector3.Up );
		if ( slopeAngle == 0 ) return;

		var heading = Vector3.Dot( GroundNormal, Rotation.Forward.WithZ( 0 ).Normal );
		var dir = Vector3.Cross( GroundNormal, Rotation.Right ).Normal;

		var left = Vector3.Cross( GroundNormal, Vector3.Up ).Normal;
		var slopeDir = Vector3.Cross( GroundNormal, left ).Normal;
		var strengthRatio = slopeAngle / 90f;
		var strength = SlopeSpeed * strengthRatio * Math.Abs( Vector3.Dot( dir, slopeDir ) );
		var velocityVector = dir * strength * Math.Sign( heading );

		var spd = Velocity.Length;
		spd += strength * Time.Delta * Math.Sign( heading );

		Velocity = dir * spd;

		if ( Debug )
		{
			DebugOverlay.Line( Position, Position + velocityVector, Color.Red, 5f );
		}
	}

	private void CheckGround()
	{
		var tr = Trace.Sphere( 3f, Position + Vector3.Up * 3f, Position + Vector3.Down * 5 )
			.Ignore( Pawn )
			.Run();

		if ( !tr.Hit )
		{
			GroundEntity = null;
			return;
		}

		GroundEntity = tr.Entity;
		GroundNormal = tr.Normal;
		groundSurface = tr.Surface.Name;

		if ( !prevGrounded )
		{
			StayOnGround();
		}
	}

	private void CheckJump()
	{
		if ( GroundEntity == null ) return;
		if ( !Input.Pressed( InputButton.Jump ) ) return;

		Velocity += Rotation.Up * JumpStrength;
		GroundEntity = null;
	}

	private void Gravity()
	{
		if ( GroundEntity != null )
		{
			if( Vector3.GetAngle(GroundNormal, Vector3.Up) <= 5 )
			{
				Velocity = Velocity.WithZ( 0 );
			}
			return;
		}

		Velocity -= new Vector3( 0, 0, 800f * Time.Delta );
	}

	private void Friction()
	{
		var speed = Velocity.Length;
		if ( speed < 0.1f ) return;

		var drop = speed * Time.Delta * .1f * GetSurfaceFriction();
		var newspeed = Math.Max( speed - drop, 0 );

		if ( newspeed != speed )
		{
			newspeed /= speed;
			Velocity *= newspeed;
		}
	}

	private float GetSurfaceFriction()
	{
		// todo: snow, gravel
		return groundSurface switch
		{
			"mud" => 5.0f,
			"sand" => 20.0f,
			"dirt" => 2.0f,
			_ => 1.0f,
		};
	}

	private void TryPedal()
	{
		if ( !PedalPosition.AlmostEqual( 0f, .1f ) && timeSincePedalStart > PedalResetAfter )
		{
			SetPedalTarget( 0f, PedalResetTime );
			return;
		}

		if ( Input.Pressed( InputButton.Attack1 ) && PedalPosition >= -.4f )
			SetPedalTarget( -1f, PedalTime, true );

		if ( Input.Pressed( InputButton.Attack2 ) && PedalPosition <= .4f )
			SetPedalTarget( 1f, PedalTime, true );
	}

	private void SetPedalTarget( float target, float timeToReach, bool tryBoost = false )
	{
		var prevStart = PedalPosition;
		var prevStartTime = timeSincePedalStart;

		timeSincePedalStart = 0;
		timeToReachTarget = timeToReach;
		pedalStartPosition = PedalPosition;
		pedalTargetPosition = target;

		if ( !tryBoost ) return;
		if ( GroundEntity == null ) return;
		if ( Math.Abs( prevStart ) <= .95f ) return;
		if ( prevStartTime > PedalTime ) return;

		if ( Pawn.IsLocalPawn )
		{
			new Perlin();
			// sound + particle
		}

		Velocity *= PerfectPedalMultiplier;
	}

	private void MovePedals( float delta )
	{
		PedalPosition += delta;

		// don't add velocity when pedals are returning to idle or in air..
		if ( pedalTargetPosition == 0 ) return;
		if ( GroundEntity == null ) return;

		var strengthAlpha = Math.Abs( pedalStartPosition );
		var strength = MinPedalStrength.LerpTo( MaxPedalStrength, strengthAlpha );
		var addVelocity = Rotation.Forward * strength * Math.Abs( delta );

		if ( Velocity.Length < 1f )
		{
			addVelocity = addVelocity.AddClamped( addVelocity * 999f, 1f );
		}

		Lean += new Angles( 0, 0, 15f * delta );
		Velocity += addVelocity;
	}

	Rotation FromToRotation( Vector3 aFrom, Vector3 aTo )
	{
		Vector3 axis = Vector3.Cross( aFrom, aTo );
		float angle = Vector3.GetAngle( aFrom, aTo );
		return AngleAxis( angle, axis.Normal );
	}

	Rotation AngleAxis( float aAngle, Vector3 aAxis )
	{
		aAxis = aAxis.Normal;
		float rad = aAngle * MathX.DegreeToRadian( .5f );
		aAxis *= MathF.Sin( rad );
		return new Quaternion( aAxis.x, aAxis.y, aAxis.z, MathF.Cos( rad ) );
	}

	Vector3 ClipVelocity( Vector3 vel, Vector3 norm, float overbounce = 1.0f )
	{
		var backoff = Vector3.Dot( vel, norm ) * overbounce;
		var o = vel - (norm * backoff);

		// garry: I don't totally understand how we could still
		//		  be travelling towards the norm, but the hl2 code
		//		  does another check here, so we're going to too.
		var adjust = Vector3.Dot( o, norm );
		if ( adjust >= 1.0f ) return o;

		adjust = MathF.Min( adjust, -1.0f );
		o -= norm * adjust;

		return o;
	}

}
