using Sandbox;
using Hammer;

[Library( "uf_trigger_tutorial", Description = "Enable tutorial features in this trigger" )]
//[Hammer.AutoApplyMaterial( "materials/editor/uf_trigger_fall.vmat" )]
[EntityTool( "Trigger Tutorial", "Unicycle Frenzy", "Enable tutorial features in this trigger." )]
internal partial class TutorialTrigger : BaseTrigger
{

	[Property]
	public InputActions DisplayBind { get; set; }
	[Property]
	public bool PerfectPedalGlow { get; set; }

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( other is not UnicyclePlayer pl ) return;

		pl.PerfectPedalGlow = PerfectPedalGlow;
	}

	public override void EndTouch( Entity other )
	{
		base.EndTouch( other );

		if ( other is not UnicyclePlayer pl ) return;
		
		pl.PerfectPedalGlow = false;
	}

	public enum InputActions
	{ 
		None,
		Pedal,
		Lean,
		Brake,
		Jump,
		Look,
		BrakeAndLean
	}

}
