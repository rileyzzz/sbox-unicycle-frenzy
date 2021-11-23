using System;
using System.Numerics;
using Sandbox;
using Sandbox.ScreenShake;

internal partial class UnicycleController : BasePlayerController
{

	[ConVar.Replicated( "uf_debug_nofall" )]
	public static bool NoFall { get; set; } = false;

	[ConVar.Replicated( "uf_debug_nolean" )]
	public static bool NoLean { get; set; } = false;

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
	public float PerfectPedalBoost { get; set; } = 50f;
	[Net]
	public float MaxLean { get; set; } = 35f;
	[Net]
	public float LeanSpeed { get; set; } = 80f;
	[Net]
	public float TurnSpeed { get; set; } = 5f;
	[Net]
	public float SlopeSpeed { get; set; } = 800f;
	[Net]
	public float BrakeStrength { get; set; } = 3f;
	[Net]
	public bool UseMomentum { get; set; } = false;
	[Net]
	public float StopSpeed { get; set; } = 35f;

	private string groundSurface;
	private bool prevGrounded;
	private Vector3 prevVelocity;
	private UnicycleUnstuck unstuck;

	private UnicyclePlayer pl => Pawn as UnicyclePlayer;
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
		if ( pl == null ) return;
		if ( unstuck.TestAndFix() ) return;

		SetTag( "sitting" );
		TryPedal();
		TryBrake();
		Gravity();
		CheckJump();
		DoSlope();
		Friction();

		// lerp pedals into place, adding velocity and lean
		if ( pl.TimeSincePedalStart < pl.TimeToReachTarget + Time.Delta )
		{
			var a = pl.TimeSincePedalStart / pl.TimeToReachTarget;
			a = Easing.EaseOut( a );

			var newPosition = pl.PedalStartPosition.LerpTo( pl.PedalTargetPosition, a );
			var delta = newPosition - pl.PedalPosition;

			MovePedals( delta );
		}

		// lean from input
		pl.Lean += new Angles( Input.Forward, 0, -Input.Left ) * Time.Delta * LeanSpeed;

		// momentum helps keep us straight
		if ( UseMomentum )
		{
			var recover = Math.Min( Velocity.WithZ( 0 ).Length / 125f, 1f );
			pl.Lean = Angles.Lerp( pl.Lean, Angles.Zero, recover * Time.Delta );
		}

		// do rotation if we're in the air or moving a little bit
		var spd = Velocity.WithZ( 0 ).Length;
		var grounded = GroundEntity != null;

		if ( (!grounded && spd < 50) || (grounded && spd > 20) )
		{
			var inputFwd = Rotation.LookAt( Input.Rotation.Forward.WithZ( 0 ), Vector3.Up );
			pl.TargetForward = Rotation.Slerp( pl.TargetForward, inputFwd, Time.Delta * 2f );
		}
		else
		{
			pl.TargetForward = Rotation;
		}

		var fromUp = grounded ? Vector3.Up : Rotation.Up;
		var targetUp = grounded ? GroundNormal : Vector3.Up;
		var targetRot = FromToRotation( fromUp, targetUp );
		targetRot *= pl.TargetForward;
		if ( !NoLean ) targetRot *= Rotation.From( pl.Lean );

		Rotation = Rotation.Slerp( Rotation, targetRot, Time.Delta * TurnSpeed );

		if ( grounded )
		{
			Velocity = ClipVelocity( Velocity, Rotation.Right );

			// zero velocity if on flat ground and moving slow
			if ( Velocity.Length < StopSpeed && Vector3.GetAngle( Vector3.Up, GroundNormal ) < 5f )
			{
				Velocity = 0;
			}
		}

		// go
		Move();
		CheckGround();

		if ( pl.IsServer && ShouldFall() )
			pl.Fall();

		prevGrounded = grounded;
		prevVelocity = Velocity;
	}

	private bool ShouldFall()
	{
		if ( NoFall ) return false;

		if ( GroundEntity != null && !prevGrounded )
		{
			if ( prevVelocity.z < -1000 )
				return true;
		}

		if ( prevVelocity.Length > StopSpeed )
		{
			var wallTrStart = Position;
			var wallTrEnd = wallTrStart + prevVelocity * Time.Delta;
			var tr = TraceBBox( wallTrStart, wallTrEnd, Mins, Maxs );

			if ( tr.Hit && Vector3.GetAngle( tr.Normal, Vector3.Up ) > 85f )
			{
				var d = Vector3.Dot( tr.Normal, prevVelocity );
				if ( d < -.3f )
					return true;
			}
		}

		//var spd = Velocity.WithZ( 0 ).Length;
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
		var mover = new MoveHelper( Position, Velocity );
		mover.Trace = mover.Trace.Size( Mins, Maxs ).Ignore( Pawn );
		mover.MaxStandableAngle = 75f;
		mover.TryMoveWithStep( Time.Delta, 12 );

		Position = mover.Position;
		Velocity = mover.Velocity;
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

		Velocity += velocityVector * Time.Delta;

		if ( Debug )
		{
			DebugOverlay.Line( Position, Position + velocityVector, Color.Red );
		}
	}

	private void CheckGround()
	{
		var tr = TraceBBox( Position, Position + Vector3.Down * 4f, Mins, Maxs, 3f );

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
			Position = tr.EndPos + tr.Normal * 3f;
		}
	}

	private void CheckJump()
	{
		if ( GroundEntity == null ) return;
		if ( !Input.Pressed( InputButton.Jump ) ) return;

		var up = Rotation.From( Rotation.Angles().WithRoll( 0 ) ).Up;

		pl.Lean = Angles.Zero;
		Velocity += up * JumpStrength;
		GroundEntity = null;
	}

	private void Gravity()
	{
		if ( GroundEntity != null )
		{
			if ( Vector3.GetAngle( GroundNormal, Vector3.Up ) <= 5 )
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
		if ( !pl.PedalPosition.AlmostEqual( 0f, .1f ) && pl.TimeSincePedalStart > PedalResetAfter )
		{
			SetPedalTarget( 0f, PedalResetTime );
			return;
		}

		if ( Input.Pressed( InputButton.Attack1 ) && pl.PedalPosition >= -.4f )
			SetPedalTarget( -1f, PedalTime, true );

		if ( Input.Pressed( InputButton.Attack2 ) && pl.PedalPosition <= .4f )
			SetPedalTarget( 1f, PedalTime, true );
	}

	private void TryBrake()
	{
		if ( GroundEntity == null ) return;
		if ( !Input.Down( InputButton.Duck ) ) return;

		Velocity = Velocity.LerpTo( Vector3.Zero, Time.Delta * BrakeStrength ).WithZ( Velocity.z );
	}

	private void SetPedalTarget( float target, float timeToReach, bool tryBoost = false )
	{
		var prevStart = pl.PedalPosition;
		var prevStartTime = pl.TimeSincePedalStart;

		pl.TimeSincePedalStart = 0;
		pl.TimeToReachTarget = timeToReach;
		pl.PedalStartPosition = pl.PedalPosition;
		pl.PedalTargetPosition = target;

		if ( !tryBoost ) return;
		if ( GroundEntity == null ) return;
		if ( Math.Abs( prevStart ) <= .95f ) return;
		if ( prevStartTime > PedalTime ) return;

		if ( Pawn.IsLocalPawn )
		{
			new Perlin();
			// sound + particle
		}

		Velocity += Rotation.Forward.WithZ( 0 ) * PerfectPedalBoost;
	}

	private void MovePedals( float delta )
	{
		pl.PedalPosition += delta;

		// don't add velocity when pedals are returning to idle or in air..
		if ( pl.PedalTargetPosition == 0 ) return;
		if ( GroundEntity == null ) return;

		var strengthAlpha = Math.Abs( pl.PedalStartPosition );
		var strength = MinPedalStrength.LerpTo( MaxPedalStrength, strengthAlpha );
		var addVelocity = Rotation.Forward * strength * Math.Abs( delta );

		pl.Lean += new Angles( 0, 0, 15f * delta );
		Velocity += addVelocity;

		if ( !Velocity.Length.AlmostEqual( 0 ) && Velocity.Length < StopSpeed )
		{
			Velocity *= StopSpeed / Velocity.Length;
		}
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
