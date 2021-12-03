using Hammer;
using Sandbox;
using Sandbox.Internal;

[Library( "uf_checkpoint", Description = "Defines a checkpoint where the player will respawn after falling" )]
[EditorModel( "models/editor/playerstart.vmdl", staticGreen: 100, FixedBounds = true )]
[EntityTool( "Player Checkpoint", "Unicycle Frenzy", "Defines a checkpoint where the player will respawn after falling" )]
[BoundsHelper( "mins", "maxs", false, true )]
internal partial class Checkpoint : Entity
{

	[Property( "mins", Title = "Checkpoint mins" )]
	[DefaultValue( "-75 -75 0" )]
	[Net]
	public Vector3 Mins { get; set; } = new Vector3( -75, -75, 0 );
	[Property( "maxs", Title = "Checkpoint maxs" )]
	[DefaultValue( "75 75 100" )]
	[Net]
	public Vector3 Maxs { get; set; } = new Vector3( 75, 75, 100 );

	[Property]
	public bool IsStart { get; set; }
	[Property]
	public bool IsEnd { get; set; }

	public Checkpoint()
	{
		Transmit = TransmitType.Always;
	}

	[Event.Frame]
	private void OnFrame()
	{
		var color = Color.White;
		if ( Local.Pawn is UnicyclePlayer pl && pl.Checkpoints.Contains( this ) )
			color = Color.Red;

		DebugOverlay.Box( Position + Mins, Position + Maxs, color );
	}

	[Event.Tick.Server]
	private void OnTick()
	{
		foreach ( var player in Player.All )
		{
			if ( player is not UnicyclePlayer pl ) continue;
			var bounds = new BBox( Position + Mins, Position + Maxs );
			if ( !bounds.Overlaps( pl.WorldSpaceBounds ) ) continue;

			pl.TrySetCheckpoint( this );
		}
	}

	public void GetSpawnPoint( out Vector3 position, out Rotation rotation )
	{
		var bounds = new BBox( Position + Mins, Position + Maxs );

		position = bounds.RandomPointInside.WithZ( Position.z );
		rotation = Rotation;
	}

}
