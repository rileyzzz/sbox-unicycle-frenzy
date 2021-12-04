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
	public Entity WheelPivot { get; set; }

	public Vector3 GetAssPosition()
	{
		var assAttachment = SeatModel.GetAttachment( "Ass", true );
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
		SeatModel?.Delete();
		WheelModel?.Delete();
		WheelPivot?.Delete();

		var def = UnicycleEnsemble.Default;
		var cfg = pl.Client.Components.Get<ClientConfig>();
		var ensemble = cfg.Ensemble;

		var frame = ensemble.Frame ?? def.Frame;
		var seat = ensemble.Seat ?? def.Seat;
		var wheel = ensemble.Wheel ?? def.Wheel;

		FrameModel = new ModelEntity( frame.Model );
		FrameModel.SetParent( this, null, Transform.Zero );

		SeatModel = new ModelEntity( seat.Model );
		SeatModel.SetParent( FrameModel, "Seat", Transform.Zero );

		WheelPivot = new Entity();
		WheelPivot.SetParent( FrameModel, "Wheel", Transform.Zero );

		WheelModel = new ModelEntity( wheel.Model );
		WheelModel.SetParent( WheelPivot, null, Transform.Zero );
		var hub = WheelModel.GetAttachment( "Hub", false );

		if ( hub.HasValue )
		{
			WheelModel.LocalPosition -= hub.Value.Position;
			FrameModel.LocalPosition = Vector3.Up * hub.Value.Position.z - 2;
		}
	}

	private int parthash;

	[Event.Tick.Server]
	private void CheckEnsemble()
	{
		if ( Parent is not UnicyclePlayer pl ) return;

		var cfg = pl.Client.Components.Get<ClientConfig>();
		var hash = cfg.Ensemble.GetPartsHash();

		if ( hash == parthash ) return;

		parthash = hash;
		AssembleParts();

		pl.Terry.Position = GetAssPosition();
		pl.Terry.Position -= Vector3.Up * 12; // remove this when proper ass attachment
	}

	[Event.Tick.Server]
	private void SpinWheel()
	{
		if ( Parent is not UnicyclePlayer pl ) return;
		if ( !WheelModel.IsValid() ) return;

		var radius = 12f;
		var hub = WheelModel.GetAttachment( "Hub", false );

		if ( hub.HasValue )
		{
			radius = hub.Value.Position.z;
		}

		var distance = pl.Velocity.Length;
		var angle = distance * (180f / (float)Math.PI) / radius; // todo: why is this not right?
		var dir = Math.Sign( Vector3.Dot( pl.Velocity.Normal, pl.Rotation.Forward ) );
		angle *= .5f; 
		WheelPivot.Rotation = WheelPivot.Rotation.RotateAroundAxis( Vector3.Left, angle * dir * Time.Delta );
	}

}

