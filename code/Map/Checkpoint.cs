using Hammer;
using Sandbox;
using Sandbox.Internal;
using System.Linq;

[Library("uf_checkpoint", Description = "Defines a checkpoint where the player will respawn after falling")]
[EditorModel("models/editor/playerstart.vmdl", staticGreen: 100, FixedBounds = true)]
[EntityTool("Player Checkpoint", "Unicycle Frenzy", "Defines a checkpoint where the player will respawn after falling")]
[BoundsHelper("mins", "maxs", false, true)]
internal partial class Checkpoint : ModelEntity
{

	[Property("mins", Title = "Checkpoint mins")]
	[DefaultValue("-75 -75 0")]
	[Net]
	public Vector3 Mins { get; set; } = new Vector3(-75, -75, 0);
	[Property("maxs", Title = "Checkpoint maxs")]
	[DefaultValue("75 75 100")]
	[Net]
	public Vector3 Maxs { get; set; } = new Vector3(75, 75, 100);

	[Net, Property]
	public bool IsStart { get; set; }
	[Net, Property]
	public bool IsEnd { get; set; }

	public ModelEntity FlagModel;

	public override void Spawn()
	{
		base.Spawn();

		SetupPhysicsFromAABB( PhysicsMotionType.Static, Mins, Maxs );

		Transmit = TransmitType.Always;
		CollisionGroup = CollisionGroup.Trigger;
		EnableSolidCollisions = false;
		EnableTouch = true;

	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		//var modelEnt = new ModelEntity("models/flag/flag_pole.vmdl");
		FlagModel = new ModelEntity("models/flag/flag_pole.vmdl");
		var fwdLeft = Rotation.Forward + Rotation.Left;
		var bounds = new BBox( Position + Mins, Position + Maxs );
		FlagModel.Position = bounds.ClosestPoint( Position + fwdLeft * 500 ) - fwdLeft.Normal * 5;
		FlagModel.Rotation = Rotation.LookAt( Rotation.Right );

		if (this.IsStart)
		{
			FlagModel.SetModel("models/flag/flag.vmdl");
			FlagModel.SetMaterialGroup("Green");
		}

		if (this.IsEnd)
		{
			FlagModel.SetModel("models/flag/flag.vmdl");
			FlagModel.SetMaterialGroup("Checker");
		}

	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );


		if ( other is not UnicyclePlayer pl ) return;

		if(pl.IsServer) pl.TrySetCheckpoint( this );

		if ( this.IsEnd && pl.TimerState == TimerState.Live )
		{
			pl.CompleteCourse();
		}

		if ( this.IsStart )
		{
			pl.EnterStartZone();
		}

	}

	public override void EndTouch( Entity other )
	{
		base.EndTouch( other );

		if ( other is not UnicyclePlayer pl ) return;

		if( this.IsStart )
		{
			pl.StartCourse();
		}
	}

	private bool active;
	[Event.Frame]
	private void OnFrame()
	{
		//DebugOverlay.Box( Position + Mins, Position + Maxs, color );

		if ( Local.Pawn is not UnicyclePlayer pl ) return;
		if ( this.IsEnd || this.IsStart ) return;

		var isLatestCheckpoint = pl.Checkpoints.LastOrDefault() == this;

		if ( !active && isLatestCheckpoint )
		{
			active = true;

			FlagModel.SetModel( "models/flag/flag.vmdl" );
		}
		else if( active && !isLatestCheckpoint )
		{
			active = false;

			FlagModel.SetModel( "models/flag/flag_pole.vmdl" );
		}
	}

	public void GetSpawnPoint( out Vector3 position, out Rotation rotation )
	{
		position = Position;
		rotation = Rotation;
	}

}
