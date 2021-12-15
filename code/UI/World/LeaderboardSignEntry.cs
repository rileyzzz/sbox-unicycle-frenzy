
using Sandbox.UI;

[UseTemplate]
internal class LeaderboardSignEntry : Panel
{

	public Label PlayerRank { get; set; }
	public Label PlayerName { get; set; }
	public Label CompletionTime { get; set; }

	public LeaderboardSignEntry( int rank, UnicyclePlayer pl )
	{
		PlayerRank.Text = rank.ToString();
		PlayerName.Text = pl.Client.Name;

		if ( pl.BestTime == float.MaxValue ) CompletionTime.Text = "INCOMPLETE";
		else CompletionTime.Text = CourseTimer.FormattedTime2( pl.BestTime );

		if ( pl.IsLocalPawn ) AddClass( "local" );
	}

}
