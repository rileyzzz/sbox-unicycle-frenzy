using System;
using System.Collections.Generic;
using System.Numerics;
using Sandbox;
using Sandbox.ScreenShake;

internal partial class UnicycleController : BasePlayerController
{

	[ConVar.Replicated( "uf_debug_nofall" )]
	public static bool NoFall { get; set; } = false;

	[ConVar.Replicated( "uf_debug_notilt" )]
	public static bool NoTilt { get; set; } = false;

	public float PedalTime => .75f;
	public float PedalResetAfter => 1.5f;
	public float PedalResetTime => .75f;
	public float MinPedalStrength => 10f;
	public float MaxPedalStrength => 50f;
	public float JumpStrength => 300f;
	public float PerfectPedalBoost => 50f;
	public float MaxLean => 35f;
	public float LeanSpeed => 80f;
	public float GroundTurnSpeed => 2f;
	public float AirTurnSpeed => 2f;
	public float SlopeSpeed => 800f;
	public float BrakeStrength => 3f;
	public bool UseMomentum => false;
	public float StopSpeed => 25f;
	public float SlopeTipStrength => 2.5f;

	public string GroundSurface { get; private set; }
	public bool PrevGrounded { get; private set; }
	public Vector3 PrevVelocity { get; private set; }

	private UnicyclePlayer pl => Pawn as UnicyclePlayer;
	public Vector3 Mins => new( -8, -8, 0 );
	public Vector3 Maxs => new( 8, 8, 48 );

	private UnicycleUnstuck unstuck;

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
			var intpos = new Vector3( (int)Position.x, (int)Position.y, (int)Position.z );
			DebugOverlay.Text( Position, "Speed: " + Velocity.Length );
			DebugOverlay.Text( Position + Vector3.Down * 3, "Position: " + intpos );
			DebugOverlay.Text( Position + Vector3.Down * 6, "Grounded: " + (GroundEntity != null) );
			DebugOverlay.Text( Position + Vector3.Down * 9, "GroundNormal: " + GroundNormal );
			DebugOverlay.Text( Position + Vector3.Down * 12, "Surface: " + GroundSurface );
			DebugOverlay.Text( Position + Vector3.Down * 15, "Water Level: " + Pawn.WaterLevel.Fraction );

			DebugOverlay.Line( Position, Position + Velocity, Color.Yellow );
		}
	}

	public override void Simulate()
	{
		if ( pl == null ) return;
		if ( unstuck.TestAndFix() ) return;

		var beforeGrounded = GroundEntity != null;
		var beforeVelocity = Velocity;

		CheckPedal();
		CheckBrake();
		CheckJump();
		DoFriction();
		DoSlope();
		DoTilt();
		DoRollingSound();

		// lerp pedals into place, adding velocity and lean
		if ( pl.TimeSincePedalStart < pl.TimeToReachTarget + Time.Delta )
		{
			var a = pl.TimeSincePedalStart / pl.TimeToReachTarget;
			a = Easing.EaseOut( a );

			var newPosition = pl.PedalStartPosition.LerpTo( pl.PedalTargetPosition, a );
			var delta = newPosition - pl.PedalPosition;

			MovePedals( delta );
		}

		DoRotation();
		Gravity();

		// go
		Move();
		CheckGround();

		if ( ShouldFall() )
		{
			if ( pl.IsServer )
			{
				pl.Fall();
			}

			AddEvent( "fall" );
		}

		PrevGrounded = beforeGrounded;
		PrevVelocity = beforeVelocity;
	}

	private bool ShouldFall()
	{
		if ( NoFall ) return false;

		if ( GroundEntity != null && !PrevGrounded )
		{
			if ( PrevVelocity.z < -1000 )
				return true;
		}

		if ( PrevVelocity.Length > StopSpeed )
		{
			var wallTrStart = Position;
			var wallTrEnd = wallTrStart + PrevVelocity * Time.Delta;
			var tr = TraceBBox( wallTrStart, wallTrEnd, Mins + Vector3.Up * 16, Maxs );

			if ( tr.Hit && Vector3.GetAngle( tr.Normal, Vector3.Up ) > 85f )
			{
				var d = Vector3.Dot( tr.Normal, PrevVelocity );
				if ( d < -.3f )
					return true;
			}
		}

		var ang = Rotation.Angles();
		var aroll = Math.Abs( ang.roll );
		var apitch = Math.Abs( ang.pitch );
		var maxLean = GroundEntity != null ? MaxLean : 180;

		if ( aroll > maxLean || apitch > maxLean )
			return true;

		if ( aroll + apitch > maxLean * 1.55f )
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

		// might help with hitting edges/getting a shit normal
		//FudgeNormal( ref tr );

		if ( Vector3.GetAngle( Vector3.Up, tr.Normal ) < 5f && Velocity.z > 140f )
		{
			GroundEntity = null;
			return;
		}

		if ( GroundEntity == null )
		{
			AddEvent( "land" );
			pl.Tilt = Rotation.Angles().WithYaw( 0 );
			Position = Position.WithZ( tr.EndPos.z );
		}

		GroundEntity = tr.Entity;
		GroundNormal = tr.Normal;
		GroundSurface = tr.Surface.Name;
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

	private void DoFriction()
	{
		if ( GroundEntity == null ) return;

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

	private void DoTilt()
	{
		if ( NoTilt )
		{
			pl.Tilt = Angles.Zero;
			return;
		}

		// recover tilt from momentum and input
		var recover = Math.Min( Velocity.WithZ( 0 ).Length / 125f, 1f );
		var tilt = pl.Tilt;
		tilt = Angles.Lerp( tilt, Angles.Zero, recover * Time.Delta );

		// uneven ground will make us tip if we're not attentive
		//var groundAng = FromToRotation( Vector3.Up, GroundNormal );
		//groundAng *= Rotation.LookAt( Rotation.Forward );
		//tilt += groundAng.Angles().WithYaw(0) * SlopeTipStrength * Time.Delta;

		// tilt from input
		var input = new Vector3( Input.Forward, 0, -Input.Left );
		tilt += new Angles( input.x, 0, input.z ) * LeanSpeed * Time.Delta;

		// accel and decel will make us pitch
		// we're only doing this on flat ground to avoid fucking up
		// the player's GroundNormal rotation while hitting slopes fast
		// might be a smarter solution here to have it both ways
		if ( GroundEntity != null && Vector3.GetAngle( Vector3.Up, GroundNormal ) < 5 )
		{
			var speedChange = PrevVelocity.WithZ( 0 ).Length - Velocity.WithZ( 0 ).Length;
			tilt += new Angles( speedChange * 3f * Time.Delta, 0, 0 );
		}

		pl.Tilt = tilt;
	}

	private void DoRotation()
	{
		if ( pl.TargetForward == default )
		{
			pl.TargetForward = Input.Rotation;
		}

		var spd = Velocity.WithZ( 0 ).Length;
		var grounded = GroundEntity != null;
		var inputFwd = Input.Rotation.Forward.WithZ( 0 );

		var canTurn = (!grounded && spd < StopSpeed) || (grounded && spd > StopSpeed);
		canTurn = canTurn && !inputFwd.IsNearlyEqual( pl.TargetForward.Forward.WithZ( 0 ) );

		if ( canTurn )
		{
			var inputRot = Rotation.LookAt( inputFwd );
			var turnSpeed = grounded ? GroundTurnSpeed : AirTurnSpeed;
			pl.TargetForward = Rotation.Slerp( pl.TargetForward, inputRot, turnSpeed * Time.Delta );

			Velocity = ClipVelocity( Velocity, pl.TargetForward.Right );
		}

		var targetRot = FromToRotation( Vector3.Up, !NoTilt ? GroundNormal : Vector3.Up );
		targetRot *= pl.TargetForward;
		targetRot *= Rotation.From( pl.Tilt );
		Rotation = Rotation.Slerp( Rotation, targetRot, 6.5f * Time.Delta );

		// zero velocity if on flat ground and moving slow
		if ( grounded && Velocity.Length < StopSpeed && Vector3.GetAngle( Vector3.Up, GroundNormal ) < 5f )
		{
			Velocity = 0;
		}
	}

	private void CheckJump()
	{
		if ( GroundEntity == null ) return;
		if ( !Input.Pressed( InputButton.Jump ) ) return;

		var up = Rotation.From( Rotation.Angles().WithRoll( 0 ) ).Up;

		Velocity += up * JumpStrength;
		GroundEntity = null;

		AddEvent( "jump" );
	}

	private void CheckPedal()
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

	private void CheckBrake()
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

		AddEvent( "pedal" );

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

		pl.Tilt += new Angles( 0, 0, 15f * delta );
		Velocity += addVelocity;

		if ( !Velocity.Length.AlmostEqual( 0 ) && Velocity.Length < StopSpeed )
		{
			Velocity *= StopSpeed / Velocity.Length;
		}
	}

	private float GetSurfaceFriction()
	{
		// todo: snow, gravel
		return GroundSurface switch
		{
			"mud" => 5.0f,
			"sand" => 20.0f,
			"dirt" => 2.0f,
			_ => 1.0f,
		};
	}

	private Vector3[] fudges = new Vector3[]
	{
		Vector3.Zero,
		Vector3.Zero,
		Vector3.Left,
		Vector3.Right,
		Vector3.Forward,
		Vector3.Backward,
	};

	private void FudgeNormal( ref TraceResult tr, float maxDistance = 10f )
	{
		fudges[0] = Velocity.Normal;
		fudges[1] = -Velocity.Normal;

		foreach ( var fudge in fudges )
		{
			var startPos = tr.EndPos + fudge * .01f;
			var endPos = tr.EndPos - tr.Normal * maxDistance;
			var newTr = Trace.Ray( startPos, endPos )
				.Ignore( Pawn )
				.Run();

			if ( !newTr.Hit ) continue;
			if ( newTr.Normal.IsNearlyEqual( tr.Normal ) ) continue;

			tr.Normal = newTr.Normal;

			break;
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
