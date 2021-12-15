﻿using Hammer;
using Sandbox;
using Sandbox.Internal;

[Library( "uf_checkpoint", Description = "Defines a checkpoint where the player will respawn after falling" )]
[EditorModel( "models/editor/playerstart.vmdl", staticGreen: 100, FixedBounds = true )]
[EntityTool( "Player Checkpoint", "Unicycle Frenzy", "Defines a checkpoint where the player will respawn after falling" )]
[BoundsHelper( "mins", "maxs", false, true )]
internal partial class Checkpoint : ModelEntity
{

	[Property( "mins", Title = "Checkpoint mins" )]
	[DefaultValue( "-75 -75 0" )]
	[Net]
	public Vector3 Mins { get; set; } = new Vector3( -75, -75, 0 );
	[Property( "maxs", Title = "Checkpoint maxs" )]
	[DefaultValue( "75 75 100" )]
	[Net]
	public Vector3 Maxs { get; set; } = new Vector3( 75, 75, 100 );

	[Net, Property]
	public bool IsStart { get; set; }
	[Net, Property]
	public bool IsEnd { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		SetupPhysicsFromAABB( PhysicsMotionType.Static, Mins, Maxs );

		Transmit = TransmitType.Always;
		CollisionGroup = CollisionGroup.Trigger;
		EnableSolidCollisions = false;
		EnableTouch = true;
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( other is not UnicyclePlayer pl ) return;

		if(pl.IsServer) pl.TrySetCheckpoint( this );

		if ( this.IsEnd )
		{
			pl.CompleteCourse();
		}

		if ( this.IsStart )
		{
			pl.TimerState = TimerState.InStartZone;
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

	[Event.Frame]
	private void OnFrame()
	{
		var color = Color.White;
		if ( Local.Pawn is UnicyclePlayer pl && pl.Checkpoints.Contains( this ) )
			color = Color.Red;

		DebugOverlay.Box( Position + Mins, Position + Maxs, color );
	}

	public void GetSpawnPoint( out Vector3 position, out Rotation rotation )
	{
		position = Position;
		rotation = Rotation;
	}

}
