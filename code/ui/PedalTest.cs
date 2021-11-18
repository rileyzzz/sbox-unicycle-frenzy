using Sandbox;
using Sandbox.UI;

[UseTemplate]
internal class PedalTest : Panel
{

	public Panel LeftPedal { get; set; }
	public Panel RightPedal { get; set; }
	public Panel Lean { get; set; }

	public override void Tick()
	{
		base.Tick();

		if ( Local.Pawn is not UnicyclePlayer player ) return;
		if ( player.Controller is not UnicycleController controller ) return;

		var leftTop = controller.PedalPosition.LerpInverse( 1f, -1f );
		var rightTop = controller.PedalPosition.LerpInverse( -1f, 1f );

		LeftPedal.Style.Top = new Length() { Unit = LengthUnit.Percentage, Value = leftTop * 100f };
		RightPedal.Style.Top = new Length() { Unit = LengthUnit.Percentage, Value = rightTop * 100f };

		var rollAlpha = controller.Lean.roll.LerpInverse( -30f, 30f );
		var pitchAlpha = controller.Lean.pitch.LerpInverse( 30f, -30f );

		Lean.Style.Left = new Length() { Unit = LengthUnit.Percentage, Value = rollAlpha * 100f };
		Lean.Style.Top = new Length() { Unit = LengthUnit.Percentage, Value = pitchAlpha * 100f };
	}

}
