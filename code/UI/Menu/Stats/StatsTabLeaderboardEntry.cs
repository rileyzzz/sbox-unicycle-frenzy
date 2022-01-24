using Sandbox;
using Sandbox.UI;

[UseTemplate]
internal class StatsTabLeaderboardEntry : Panel
{

	public int Rank { get; set; }
	public string Name { get; set; }
	public string Time { get; set; }

	public StatsTabLeaderboardEntry( int rank, string name, float score, long playerId = 0 )
	{
		Rank = rank;
		Name = name;
		Time = CourseTimer.FormattedTimeMsf( score );

		SetClass( "me", Local.PlayerId == playerId );
		SetClass( "friend", new Friend( playerId ).IsFriend );
	}

}
