using Sandbox.UI;

[UseTemplate]
internal class LeaderboardsTabEntry : Panel
{

	public string PlayerName { get; set; }
	public string Rank { get; set; }
	public string Time { get; set; }

	public LeaderboardsTabEntry( int rank, string name, float time )
	{
		PlayerName = name;
		Rank = rank.ToString();
		Time = System.TimeSpan.FromSeconds( time ).ToString( @"m\:ss" );
	}

}

