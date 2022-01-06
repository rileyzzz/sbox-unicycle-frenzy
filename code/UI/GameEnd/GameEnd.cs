using Sandbox;
using Sandbox.UI;

[UseTemplate]
internal class GameEnd : Panel
{

	public Panel MapCanvas { get; set; }

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
		SetClass( "open", UnicycleFrenzy.Game.TimeLeft < 1 * 60 );
	}

}
