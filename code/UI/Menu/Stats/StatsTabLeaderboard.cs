using Sandbox;
using Sandbox.UI;

[UseTemplate]
[NavigatorTarget( "menu/stats/leaderboard" )]
internal class StatsTabLeaderboard : NavigatorPanel
{

	public StatsTabLeaderboard()
	{
		Navigate( "/menu/stats/leaderboard/global" );
	}

}

