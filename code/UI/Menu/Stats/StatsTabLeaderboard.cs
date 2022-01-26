using Sandbox;
using Sandbox.UI;

[UseTemplate]
[NavigatorTarget( "menu/stats/leaderboard" )]
internal class StatsTabLeaderboard : NavigatorPanel
{

	public string MapName => Global.MapName;

	public StatsTabLeaderboard()
	{
		Navigate( "/menu/stats/leaderboard/session" );
	}

}

