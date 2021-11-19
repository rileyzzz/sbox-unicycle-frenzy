using Sandbox;
using Sandbox.UI;

[UseTemplate]
internal class PedalTest : Panel
{

	public Panel LeftPedal { get; set; }
	public Panel RightPedal { get; set; }
	public Panel AbsLean { get; set; }
	public Panel LocalLean { get; set; }

	public override void Tick()
	{
		base.Tick();

		if ( Local.Pawn is not UnicyclePlayer player ) return;
		if ( player.Controller is not UnicycleController controller ) return;

		var leftTop = controller.PedalPosition.LerpInverse( 1f, -1f );
		var rightTop = controller.PedalPosition.LerpInverse( -1f, 1f );

		LeftPedal.Style.Top = new Length() { Unit = LengthUnit.Percentage, Value = leftTop * 100f };
		RightPedal.Style.Top = new Length() { Unit = LengthUnit.Percentage, Value = rightTop * 100f };

		var absLean = controller.Rotation.Angles();
		var absRollAlpha = absLean.roll.LerpInverse( -30f, 30f );
		var absPitchAlpha = absLean.pitch.LerpInverse( 30f, -30f );

		AbsLean.Style.Left = new Length() { Unit = LengthUnit.Percentage, Value = absRollAlpha * 100f };
		AbsLean.Style.Top = new Length() { Unit = LengthUnit.Percentage, Value = absPitchAlpha * 100f };

		var localLean = controller.Lean;
		var localRollAlpha = localLean.roll.LerpInverse( -30f, 30f );
		var localPitchAlpha = localLean.pitch.LerpInverse( 30f, -30f );

		LocalLean.Style.Left = new Length() { Unit = LengthUnit.Percentage, Value = localRollAlpha * 100f };
		LocalLean.Style.Top = new Length() { Unit = LengthUnit.Percentage, Value = localPitchAlpha * 100f };
	}

}
