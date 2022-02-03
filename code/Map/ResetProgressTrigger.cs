using Sandbox;
using Hammer;

[Library( "uf_trigger_reset_progress", Description = "Resets the player's progress by clearing checkpoints" )]
[Hammer.AutoApplyMaterial("materials/editor/uf_trigger_reset_progress.vmat")]
[EntityTool( "Reset Progress", "Unicycle Frenzy", "" )]
internal partial class ResetProgressTrigger : BaseTrigger
{

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( !other.IsServer ) return;
		if ( other is not UnicyclePlayer pl ) return;

		pl.ClearCheckpoints();
	}

}
