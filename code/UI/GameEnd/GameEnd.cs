using Sandbox;
using Sandbox.UI;
using System;
using System.Linq;

[UseTemplate]
internal class GameEnd : Panel
{

	public Panel MapCanvas { get; set; }
	public string TimeLeft => CourseTimer.FormattedTimeMs( UnicycleFrenzy.Game.TimeLeft );

	public string FirstName { get; set; }
	public string FirstTime { get; set; }
	public string SecondName { get; set; }
	public string SecondTime { get; set; }
	public string ThirdName { get; set; }
	public string ThirdTime { get; set; }

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

		foreach ( var map in UnicycleFrenzy.Game.MapCycle )
		{
			var btn = new MapVoteButton( map );
			MapCanvas.AddChild( btn );
		}
	}

	private int maphash = 0;
		
	[Event.Frame]
	private void OnFrame()
	{
		//var open = UnicycleFrenzy.Game.TimeLeft < UnicycleFrenzy.EndGameDuration;

		//SetClass( "open", open );

		//if ( !open ) return;

		//EnsureMaps();

		//var players = Player.All.Where( x => x is UnicyclePlayer && x.IsValid() && x.Client.IsValid() ).ToList();
		//var orderedPlayers = players.OrderBy( x => (x as UnicyclePlayer).BestTime );

		//int rank = 1;

		//foreach ( var player in orderedPlayers )
		//{
		//	var pl = player as UnicyclePlayer;
		//	switch ( rank )
		//	{
		//		case 1:
		//			FirstName = $"#1 " + pl.Client.Name;
		//			FirstTime = pl.CourseIncomplete ? "INCOMPLETE" : CourseTimer.FormattedTimeMsf( pl.BestTime );
		//			break;
		//		case 2:
		//			SecondName = $"#2 " + pl.Client.Name;
		//			SecondTime = pl.CourseIncomplete ? "INCOMPLETE" : CourseTimer.FormattedTimeMsf( pl.BestTime );
		//			break;
		//		case 3:
		//			ThirdName = $"#3 " + pl.Client.Name;
		//			ThirdTime = pl.CourseIncomplete ? "INCOMPLETE" : CourseTimer.FormattedTimeMsf( pl.BestTime );
		//			break;
		//		default: 
		//			return;
		//	}
		//	rank++;
		//}
	}

	private void EnsureMaps()
	{
		var newhash = 0;
		foreach ( var map in UnicycleFrenzy.Game.MapCycle )
		{
			newhash = HashCode.Combine( map );
		}

		if ( newhash != maphash )
		{
			RefreshMaps();
			maphash = newhash;
		}
	}

}
