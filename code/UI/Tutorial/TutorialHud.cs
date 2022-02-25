using Sandbox;
using Sandbox.UI;

[UseTemplate]
internal class TutorialHud : Panel
{

	public Panel ControlHints { get; set; }
	public ControlHint LeftHint { get; set; }
	public ControlHint MiddleHint { get; set; }
	public ControlHint RightHint { get; set; }

	public override void Tick()
	{
		base.Tick();

		if ( Local.Pawn is not UnicyclePlayer pl )
		{
			ControlHints.SetClass( "open", false );
			return;
		}

		if( pl.DisplayedAction == TutorialTrigger.InputActions.None )
		{
			ControlHints.SetClass( "open", false );
			return;
		}

		ControlHints.SetClass( "open", true );
		LeftHint.SetClass( "open", false );
		RightHint.SetClass( "open", false );
		MiddleHint.SetClass( "open", false );

		switch ( pl.DisplayedAction )
		{
			case TutorialTrigger.InputActions.Pedal:
				LeftHint.Set( "Left Pedal", InputButton.Attack1 );
				RightHint.Set( "Right Pedal", InputButton.Attack2 );
				break;
			case TutorialTrigger.InputActions.Brake:
				MiddleHint.Set( "Brake", InputButton.Walk );
				break;
			case TutorialTrigger.InputActions.Jump:
				MiddleHint.Set( "Jump", InputButton.Jump );
				break;
			case TutorialTrigger.InputActions.Look:
				MiddleHint.Set( "Look/Steer", pl.DisplayedAction );
				break;
			case TutorialTrigger.InputActions.Lean:
				MiddleHint.Set( "Lean", pl.DisplayedAction );
				break;
			case TutorialTrigger.InputActions.BrakeAndLean:
				LeftHint.Set( "Brake", InputButton.Run );
				RightHint.Set( "Lean", TutorialTrigger.InputActions.Lean );
				break;
			case TutorialTrigger.InputActions.JumpHigher:
				MiddleHint.Set( "Hold to charge jump", InputButton.Jump, true );
				break;
			default:
				MiddleHint.Set( "UNKNOWN: " + pl.DisplayedAction.ToString(), InputButton.Jump );
				break;
		}
	}

}
