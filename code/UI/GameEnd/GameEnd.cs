
using Sandbox;
using Sandbox.UI;
using System;

[UseTemplate]
internal class GameEnd : Panel
{

	public Panel MapCanvas { get; set; }
	public Panel PodiumCanvas { get; set; }
	public string TimeLeft => CourseTimer.FormattedTimeMs( UnicycleFrenzy.Game.StateTimer );

	[Event.Frame]
	private void OnFrame()
	{
<<<<<<< HEAD
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
=======
		var open = UnicycleFrenzy.Game.GameState == UnicycleFrenzy.GameStates.End;

		SetClass( "open", open );

		if ( !open ) return;

		EnsureMaps();
>>>>>>> 4846b82009dbeb1080bd32b7f034aef2be3c98ae
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
		Refresh();
	}

	private void Refresh()
	{
		PodiumCanvas.DeleteChildren( true );
		PodiumCanvas.AddChild( new PodiumPanel( 2 ) );
		PodiumCanvas.AddChild( new PodiumPanel( 1 ) );
		PodiumCanvas.AddChild( new PodiumPanel( 3 ) );

		MapCanvas.DeleteChildren( true );
		foreach ( var map in UnicycleFrenzy.Game.MapOptions )
		{
			var btn = new MapVoteButton( map );
			MapCanvas.AddChild( btn );
		}
	}

	protected override void PostTemplateApplied() => Refresh();
	public override void OnHotloaded() => Refresh();

}
