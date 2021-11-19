using Hammer;
using Sandbox;

[EditorModel( "models/checkpoint_flag.vmdl" )]
[Library( "uf_checkpoint", Description = "Sets a checkpoint when you come near it" )]
internal class CheckpointFlag : ModelEntity
{

	[Property( "radius", Title = "Checkpoint Radius" )]
	public float Radius { get; set; } = 100f;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/checkpoint_flag.vmdl" );
	}

	[Event.Tick.Server]
	private void OnTick()
	{
		foreach ( var player in Player.All )
		{
			if ( player is not UnicyclePlayer pl ) continue;
			if ( pl.Position.Distance( Position ) > Radius ) continue;

			pl.TrySetCheckpoint( this );
		}
	}

}
