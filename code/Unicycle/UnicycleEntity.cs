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

		var cfg = pl.Client.Components.Get<UnicycleEnsemble>();

		var frame = cfg.GetPart( PartType.Frame );
		var seat = cfg.GetPart( PartType.Seat );
		var wheel = cfg.GetPart( PartType.Wheel );
		var pedal = cfg.GetPart( PartType.Pedal );

		FrameModel = new ModelEntity( frame.Model );
		FrameModel.SetParent( this, null, Transform.Zero );

		SeatModel = new ModelEntity( seat.Model );
		SeatModel.SetParent( FrameModel, true );

		WheelModel = new ModelEntity( wheel.Model );
		WheelModel.SetParent( FrameModel, true );

		FrameModel.LocalPosition = Vector3.Up * GetWheelRadius();

		PedalsPivot = new Entity();
		PedalsPivot.SetParent( FrameModel, "pedals", Transform.Zero );

		LeftPedalModel = new ModelEntity( pedal.Model );
		LeftPedalModel.SetParent( PedalsPivot, null, Transform.Zero );

		RightPedalModel = new ModelEntity( pedal.Model );
		RightPedalModel.SetParent( PedalsPivot, null, Transform.Zero );

		LeftPedalModel.Transform = FrameModel.GetBoneTransform( "pedal_L" );
		RightPedalModel.Transform = FrameModel.GetBoneTransform( "pedal_R" );

		var pedalOffset = LeftPedalModel.GetBoneTransform( "hub" ).Position.z - FrameModel.GetBoneTransform( "pedal" ).Position.z;
		LeftPedalModel.LocalPosition += Vector3.Down * pedalOffset;
		RightPedalModel.LocalPosition += Vector3.Up * pedalOffset;

		PedalsPivot.LocalRotation = PedalsPivot.LocalRotation.RotateAroundAxis( Vector3.Right, 90 );
	}

	private int parthash = -1;

	[Event.Tick.Server]
	private void CheckEnsemble()
	{
		if ( Parent is not UnicyclePlayer pl ) return;

		var cfg = pl.Client.Components.Get<UnicycleEnsemble>();
		var hash = cfg.GetPartsHash();

		if ( hash == parthash ) return;

		parthash = hash;
		AssembleParts();

		pl.Terry.Position = GetAssPosition();
		pl.Terry.Position -= Vector3.Up * 12; // remove this when proper ass attachment
	}

	private float GetWheelRadius()
	{
		if ( !WheelModel.IsValid() ) return 12f;

		var hubBone = WheelModel.GetBoneTransform( "hub" );
		var wheelBone = WheelModel.GetBoneTransform( "wheel" );

		if ( hubBone == Transform.Zero || wheelBone == Transform.Zero ) return 12f;

		return hubBone.Position.z - wheelBone.Position.z;
	}

	// todo: either I'm using bones wrong or setting bones via code is fucked
	//[Event.Frame]
	//private void SpinParts()
	//{
	//	if ( Parent is not UnicyclePlayer pl ) return;
	//	if ( !FrameModel.IsValid() ) return;

	//	var radius = GetWheelRadius();
	//	var distance = pl.Velocity.Length;
	//	var angle = distance * (180f / (float)Math.PI) / radius;
	//	var dir = Math.Sign( Vector3.Dot( pl.Velocity.Normal, pl.Rotation.Forward ) );

	//	var tx = FrameModel.GetBoneTransform( "hub", false );
	//	tx.Rotation = tx.Rotation.RotateAroundAxis( Vector3.Right, angle * dir * Time.Delta );

	//	FrameModel.SetBoneTransform( "hub", tx, false );
	//}

	[Event.Tick.Server]
	private void SpinPedals()
	{
		if ( Parent is not UnicyclePlayer pl ) return;

		var pedalAlpha = pl.PedalPosition.LerpInverse( -1f, 1f );
		var targetPitch = 0f.LerpTo( 180, pedalAlpha );
		var targetRot = Rotation.From( targetPitch, 0, 0 );

		var ang = targetRot.Angle() - PedalsPivot.LocalRotation.Angle();
		PedalsPivot.LocalRotation = PedalsPivot.LocalRotation.RotateAroundAxis( Vector3.Left, Math.Abs( ang ) );
	}

}

