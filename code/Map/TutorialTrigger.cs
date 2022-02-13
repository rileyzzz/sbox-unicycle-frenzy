using Sandbox;
using Hammer;

[Library( "uf_trigger_tutorial", Description = "Enable tutorial features in this trigger" )]
//[Hammer.AutoApplyMaterial( "materials/editor/uf_trigger_fall.vmat" )]
[EntityTool( "Trigger Tutorial", "Unicycle Frenzy", "Enable tutorial features in this trigger." )]
internal partial class TutorialTrigger : BaseTrigger
{

	[Property]
	public bool PerfectPedalGlow { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );


	}

	public override void EndTouch( Entity other )
	{
		base.EndTouch( other );


	}

}
