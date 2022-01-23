using Sandbox.UI;

[UseTemplate]
internal class StatsTabLeaderboardEntry : Panel
{

	public int Rank { get; set; }
	public string Name { get; set; }
	public string Time { get; set; }

	public StatsTabLeaderboardEntry( int rank, string name, float score )
	{
		Rank = rank;
		Name = name;
		Time = CourseTimer.FormattedTimeMsf( score );
	}

}
