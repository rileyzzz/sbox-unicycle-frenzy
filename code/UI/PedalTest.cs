using Sandbox;
using Sandbox.UI;
using System;

[UseTemplate]
internal class PedalTest : Panel
{

	public Panel LeftPedal { get; set; }
	public Panel RightPedal { get; set; }
	public Panel Jump { get; set; }
	public Panel AbsLean { get; set; }
	public Panel LocalLean { get; set; }

	public override void Tick()
	{
		base.Tick();

		if ( Local.Pawn is not UnicyclePlayer player ) return;
		if ( player.Controller is not UnicycleController controller ) return;

		var leftTop = player.PedalPosition.LerpInverse( 1f, -1f );
		var rightTop = player.PedalPosition.LerpInverse( -1f, 1f );
		var maxLean = controller.MaxLean;

		LeftPedal.Style.Top = new Length() { Unit = LengthUnit.Percentage, Value = leftTop * 100f };
		RightPedal.Style.Top = new Length() { Unit = LengthUnit.Percentage, Value = rightTop * 100f };

		var jumpStrength = Math.Min( player.TimeSinceJumpDown / controller.MaxJumpStrengthTime, 1f );
		if ( !Input.Down( InputButton.Jump ) ) jumpStrength = 0;
		jumpStrength = Easing.EaseOut( jumpStrength );
		Jump.Style.Height = new Length() { Unit = LengthUnit.Percentage, Value = jumpStrength * 100f };

		var absLean = player.Rotation.Angles();
		var absRollAlpha = absLean.roll.LerpInverse( -maxLean, maxLean );
		var absPitchAlpha = absLean.pitch.LerpInverse( maxLean, -maxLean );

		AbsLean.Style.Left = new Length() { Unit = LengthUnit.Percentage, Value = absRollAlpha * 100f };
		AbsLean.Style.Top = new Length() { Unit = LengthUnit.Percentage, Value = absPitchAlpha * 100f };

		var localLean = player.Tilt;
		var localRollAlpha = localLean.roll.LerpInverse( -maxLean, maxLean );
		var localPitchAlpha = localLean.pitch.LerpInverse( maxLean, -maxLean );

		LocalLean.Style.Left = new Length() { Unit = LengthUnit.Percentage, Value = localRollAlpha * 100f };
		LocalLean.Style.Top = new Length() { Unit = LengthUnit.Percentage, Value = localPitchAlpha * 100f };
	}

}
