using Sandbox;
using Sandbox.UI;

[UseTemplate]
internal class GameMenu : NavigatorPanel
{

	public GameMenu()
	{
		Navigate( "menu/stats" );
	}

	public void Close()
	{
		SetClass( "open", false );
	}

	[Event.BuildInput]
	private void BuildInput( InputBuilder b )
	{
		var btn = InputActionsExtensions.GetInputButton( InputActions.Menu );
		if ( b.Pressed( btn ) )
		{
			SetClass( "open", !HasClass( "open" ) );
		}
	}

	public override void Tick()
	{
		base.Tick();

		if ( Local.Pawn is not UnicyclePlayer pl ) return;

		SetClass( "is-spectating", pl.SpectateTarget != null );
	}

	public void StopSpectating()
	{
		UnicyclePlayer.ServerCmd_SetSpectateTarget( -1 );
		Close();
	}

}

