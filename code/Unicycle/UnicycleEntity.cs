using Sandbox;
using System;

internal partial class UnicycleEntity : Entity
{

	[Net]
	public ModelEntity FrameModel { get; set; }
	[Net]
	public ModelEntity SeatModel { get; set; }
	[Net]
	public ModelEntity WheelModel { get; set; }
	[Net]
	public ModelEntity LeftPedalModel { get; set; }
	[Net]
	public ModelEntity RightPedalModel { get; set; }
	[Net]
	public Entity PedalsPivot { get; set; }
	[Net]
	public Entity WheelPivot { get; set; }
	[Net]
	public float WheelRadius { get; set; }

	private Particles trailParticle;

	public Vector3 GetAssPosition()
	{
		var assAttachment = SeatModel.GetAttachment( "Ass" );
		if ( !assAttachment.HasValue )
		{
			return Vector3.Zero;
		}

		return assAttachment.Value.Position;
	}

	private void AssembleParts()
	{
		Host.AssertServer();

		if ( Parent is not UnicyclePlayer pl ) return;

		FrameModel?.Delete();
		FrameModel = null;
		trailParticle?.Destroy();
		trailParticle = null;

		var cfg = pl.Client.Components.Get<UnicycleEnsemble>();

		var frame = cfg.GetPart( PartType.Frame );
		var seat = cfg.GetPart( PartType.Seat );
		var wheel = cfg.GetPart( PartType.Wheel );
		var pedal = cfg.GetPart( PartType.Pedal );
		var trail = cfg.GetPart( PartType.Trail );

		FrameModel = new ModelEntity( frame.Model );
		FrameModel.SetParent( this, null, Transform.Zero );

		SeatModel = new ModelEntity( seat.Model );
		SeatModel.SetParent( FrameModel, "seat", Transform.Zero );

		WheelPivot = new Entity();
		WheelPivot.SetParent( FrameModel, "hub", Transform.Zero );

		WheelModel = new ModelEntity( wheel.Model );
		WheelModel.SetParent( WheelPivot, null, Transform.Zero );

		var wheelHub = WheelModel.GetAttachment( "hub" ) ?? Transform.Zero;
		WheelRadius = wheelHub.Position.z - WheelModel.Position.z;

		WheelModel.LocalPosition -= Vector3.Up * WheelRadius;
		FrameModel.LocalPosition = Vector3.Up * WheelRadius;

		PedalsPivot = new Entity();
		PedalsPivot.SetParent( FrameModel, "hub", Transform.Zero );

		LeftPedalModel = new ModelEntity( pedal.Model );
		LeftPedalModel.SetParent( PedalsPivot, null, Transform.Zero );

		RightPedalModel = new ModelEntity( pedal.Model );
		RightPedalModel.SetParent( PedalsPivot, null, Transform.Zero );

		var pedalHub = LeftPedalModel.GetAttachment( "hub", false ) ?? Transform.Zero;

		LeftPedalModel.Position = (FrameModel.GetAttachment( "pedal_L" ) ?? Transform.Zero).Position;
		RightPedalModel.Position = (FrameModel.GetAttachment( "pedal_R" ) ?? Transform.Zero).Position;

		LeftPedalModel.LocalPosition -= pedalHub.Position;
		RightPedalModel.LocalPosition += pedalHub.Position;
		RightPedalModel.LocalRotation *= Rotation.From( 180, 180, 0 );

		PedalsPivot.LocalRotation = PedalsPivot.LocalRotation.RotateAroundAxis( Vector3.Right, 90 );

		if( trail != null )
		{
			trailParticle = Particles.Create( trail.Model, this );
		}
	}

	private int parthash = -1;

	[Event.Tick.Server]
	private void CheckEnsemble()
	{
		if ( Parent is not UnicyclePlayer pl ) return;
		if ( !pl.IsValid() || !pl.Client.IsValid() ) return;

		var cfg = pl.Client.Components.Get<UnicycleEnsemble>();
		var hash = cfg.GetPartsHash();

		if ( hash == parthash ) return;

		parthash = hash;
		AssembleParts();

		pl.Terry.Position = GetAssPosition();
		pl.Terry.Position -= Vector3.Up * 12; // remove this when proper ass attachment
	}

	[Event.Tick.Server]
	private void SpinParts()
	{
		if ( Parent is not UnicyclePlayer pl ) return;

		var pedalAlpha = pl.PedalPosition.LerpInverse( -1f, 1f );
		var targetPitch = 0f.LerpTo( 180, pedalAlpha );
		var targetRot = Rotation.From( targetPitch, 0, 0 );

		var ang = targetRot.Angle() - PedalsPivot.LocalRotation.Angle();
		PedalsPivot.LocalRotation = PedalsPivot.LocalRotation.RotateAroundAxis( Vector3.Left, Math.Abs( ang ) );

		var angularSpeed = 180f * pl.Velocity.WithZ( 0 ).Length / ((float)Math.PI * WheelRadius);
		var dir = Math.Sign( Vector3.Dot( pl.Velocity.Normal, pl.Rotation.Forward ) );

		WheelPivot.LocalRotation = WheelPivot.LocalRotation.RotateAroundAxis( Vector3.Left, angularSpeed * dir * Time.Delta );
	}

	[Event.Tick.Server]
	private void SetTrailControlPoint()
	{
		if ( Parent is not UnicyclePlayer pl ) return;
		if ( trailParticle == null ) return;

		var a = Math.Min( pl.Velocity.Length / 500f, 1f );
		trailParticle.SetPosition( 6, new Vector3( a, 0, 0 ) );
	}

}

