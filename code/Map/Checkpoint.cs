using Hammer;
using Sandbox;
using System.Linq;

[Library("uf_checkpoint", Description = "Defines a checkpoint where the player will respawn after falling")]
[EditorModel( "models/checkpoint_platform_hammer.vmdl", FixedBounds = true)]
[EntityTool("Player Checkpoint", "Unicycle Frenzy", "Defines a checkpoint where the player will respawn after falling")]
internal partial class Checkpoint : ModelEntity
{

	[Net, Property]
	public bool IsStart { get; set; }
	[Net, Property]
	public bool IsEnd { get; set; }

	private ModelEntity flag;

	public enum ModelType
	{
		Dev,
		Metal,
		Stone,
		Wood
	}

	/// <summary>
	/// Movement type of the door.
	/// </summary>
	[Property("model_type", Title = "Model Type")]
	public ModelType ModelTypeList { get; set; } = ModelType.Dev;

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		EnableAllCollisions = true;
		EnableTouch = true;

		if (ModelTypeList == ModelType.Dev)
		{
			SetModel("models/checkpoint_platform.vmdl");
		}

		else if (ModelTypeList == ModelType.Metal)
		{
			SetModel("models/checkpoint_platform_metal.vmdl");
		}

		else if (ModelTypeList == ModelType.Stone)
		{
			SetModel("models/checkpoint_platform_stone.vmdl");
		}

		else if (ModelTypeList == ModelType.Wood)
		{
			SetModel("models/checkpoint_platform_wood.vmdl");
		}

		SetupPhysicsFromModel( PhysicsMotionType.Static );

		var bounds = PhysicsBody.GetBounds();
		var extents = ( bounds.Maxs - bounds.Mins ) * 0.5f;

		var trigger = new BaseTrigger();
		trigger.SetParent( this, null, Transform.Zero );
		trigger.SetupPhysicsFromAABB( PhysicsMotionType.Static, -extents.WithZ( 0 ), extents.WithZ( 128 ) );
		trigger.Transmit = TransmitType.Always;


	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		var flagAttachment = GetAttachment( "Flag" );

		flag = new ModelEntity( "models/flag/flag_pole.vmdl" );
		flag.Position = flagAttachment.Value.Position;
		flag.Rotation = flagAttachment.Value.Rotation;

		if ( this.IsStart )
		{
			flag.SetModel( "models/flag/flag.vmdl" );
			flag.SetMaterialGroup( "Green" );
		}

		if ( this.IsEnd )
		{
			flag.SetModel( "models/flag/flag.vmdl" );
			flag.SetMaterialGroup( "Checker" );
		}
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( other is not UnicyclePlayer pl ) return;

		if( pl.IsServer ) pl.TrySetCheckpoint( this );

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
		if ( Local.Pawn is not UnicyclePlayer pl ) return;
		if ( this.IsEnd || this.IsStart ) return;

		var isLatestCheckpoint = pl.Checkpoints.LastOrDefault() == this;

		if ( !active && isLatestCheckpoint )
		{
			active = true;

			flag.SetModel( "models/flag/flag.vmdl" );

			Juice.Scale( 1f, 1.25f, 1f )
				.WithDuration( .5f )
				.WithEasing( EasingType.BounceOut )
				.WithTarget( flag );
		}
		else if( active && !isLatestCheckpoint )
		{
			active = false;

			flag.SetModel( "models/flag/flag_pole.vmdl" );
		}
	}

	public void GetSpawnPoint( out Vector3 position, out Rotation rotation )
	{
		position = Position;
		rotation = Rotation;
	}

}
