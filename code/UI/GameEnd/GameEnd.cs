using Sandbox;
using Sandbox.UI;

[UseTemplate]
internal class GameEnd : Panel
{

	public Panel MapCanvas { get; set; }
	public string TimeLeft => CourseTimer.FormattedTimeMs( UnicycleFrenzy.Game.TimeLeft );

	protected override void PostTemplateApplied()
	{
		base.PostTemplateApplied();

		RefreshMaps();
	}

	public override void OnHotloaded()
	{
		base.OnHotloaded();

		RefreshMaps();
	}

	private void RefreshMaps()
	{
		MapCanvas.DeleteChildren();

		foreach ( var map in UnicycleFrenzy.MapCycle )
		{
			var btn = new MapVoteButton( map );
			MapCanvas.AddChild( btn );
		}
	}
		
	[Event.Frame]
	private void OnFrame()
	{
		SetClass( "open", UnicycleFrenzy.Game.TimeLeft < UnicycleFrenzy.EndGameDuration );
	}

}
