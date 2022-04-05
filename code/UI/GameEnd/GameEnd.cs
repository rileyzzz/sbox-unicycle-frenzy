using Sandbox;
using Sandbox.UI;
using System;

[UseTemplate]
internal class GameEnd : Panel
{

	public Panel MapCanvas { get; set; }
	public string TimeLeft => CourseTimer.FormattedTimeMs( UnicycleFrenzy.Game.GameTimer );

	[Event.Frame]
	private void OnFrame()
	{
		var open = UnicycleFrenzy.Game.GameTimer < UnicycleFrenzy.EndGameDuration;

		SetClass( "open", open );

		if ( !open ) return;

		EnsureMaps();
	}

	private int maphash = 0;
	private void EnsureMaps()
	{
		var newhash = 0;
		var options = UnicycleFrenzy.Game.MapOptions;

		foreach ( var map in options )
		{
			newhash = HashCode.Combine( newhash, map );
		}

		if ( newhash == maphash ) return;

		maphash = newhash;
		RefreshMaps();
	}

	private void RefreshMaps()
	{
		MapCanvas.DeleteChildren( true );

		foreach ( var map in UnicycleFrenzy.Game.MapOptions )
		{
			var btn = new MapVoteButton( map );
			MapCanvas.AddChild( btn );
		}
	}

	protected override void PostTemplateApplied() => RefreshMaps();
	public override void OnHotloaded() => RefreshMaps();

}
