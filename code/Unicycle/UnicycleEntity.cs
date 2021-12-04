using Sandbox;
using System;

internal partial class UnicycleEntity : Entity
{

	public string FramePath = "models/parts/frames/dev_frame";
	public string SeatPath = "models/parts/seats/dev_seat";
	public string WheelPath = "models/parts/wheels/dev_wheel";

	[Net]
	public ModelEntity FrameModel { get; set; }
	[Net]
	public ModelEntity SeatModel { get; set; }
	[Net]
	public ModelEntity WheelModel { get; set; }
	[Net]
	public Entity WheelPivot { get; set; }

	public Vector3 GetAssLocalPosition()
	{
		var assAttachment = SeatModel.GetAttachment( "Ass", true );
		if ( !assAttachment.HasValue )
		{
			return Vector3.Zero;
		}

		return assAttachment.Value.Position - Position;
	}

	public override void Spawn()
	{
		base.Spawn();

		Assemble();
	}

	public void Assemble()
	{
		Host.AssertServer();

		FrameModel?.Delete();
		SeatModel?.Delete();
		WheelModel?.Delete();
		WheelPivot?.Delete();

		FrameModel = new ModelEntity( FramePath );
		FrameModel.SetParent( this, null, Transform.Zero.WithScale( 1 ) );

		SeatModel = new ModelEntity( SeatPath );
		SeatModel.SetParent( FrameModel, "Seat", Transform.Zero.WithScale( 1 ) );

		WheelPivot = new Entity();
		WheelPivot.SetParent( FrameModel, "Wheel", Transform.Zero.WithScale( 1 ) );

		WheelModel = new ModelEntity( WheelPath );
		WheelModel.SetParent( WheelPivot, null, Transform.Zero.WithScale( 1 ) );
		var hub = WheelModel.GetAttachment( "Hub", false );

		if ( hub.HasValue )
		{
			WheelModel.LocalPosition -= hub.Value.Position;
			FrameModel.LocalPosition = Vector3.Up * hub.Value.Position.z - 2;
		}
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

