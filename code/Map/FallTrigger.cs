using Sandbox;
using Hammer;

[Library( "uf_trigger_fall", Description = "Makes the player fall" )]
[Hammer.AutoApplyMaterial("materials/editor/uf_trigger_fall.vmat")]
[EntityTool( "Trigger Fall", "Unicycle Frenzy", "Makes the player fall." )]
internal partial class FallTrigger : BaseTrigger
{

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( !other.IsServer ) return;
		if ( other is not UnicyclePlayer pl ) return;

		pl.Fall();
	}

}
