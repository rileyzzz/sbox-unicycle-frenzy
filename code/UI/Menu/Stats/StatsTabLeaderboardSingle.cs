using Sandbox.UI;
using System;
using System.Linq;

[UseTemplate]
[NavigatorTarget( "menu/stats/leaderboard/{scope}" )]
internal class StatsTabLeaderboardSingle : NavigatorPanel
{

	private enum LeaderboardScope
	{
		None,
		Session,
		Global,
		Friends
	}

	private LeaderboardScope scope = LeaderboardScope.None;

	public Panel Canvas { get; set; }

	public override void SetProperty( string name, string value )
	{
		base.SetProperty( name, value );

		if( string.Equals(name, "scope" ) )
		{
			scope = Enum.Parse<LeaderboardScope>( value, true );

			RebuildLeaderboard();
		}
	}

	protected override void PostTemplateApplied()
	{
		base.PostTemplateApplied();

		RebuildLeaderboard();
	}

	public override void OnHotloaded()
	{
		base.OnHotloaded();

		RebuildLeaderboard();
	}

	public void Rebuild()
	{
		RebuildLeaderboard();
	}

	private async void RebuildLeaderboard()
	{
		if ( scope == LeaderboardScope.None ) return;

		Canvas.DeleteChildren( true );

		switch ( scope )
		{
			case LeaderboardScope.Global:
				var q = await GameServices.Leaderboard.Query( game: Global.GameName, bucket: Global.MapName );
				var sorted = q.Entries.OrderBy( x => x.Rating );
				var rank = 1;

				foreach ( var entry in sorted )
				{
					var el = new StatsTabLeaderboardEntry( rank, entry.DisplayName, entry.Rating );
					el.Parent = Canvas;
					rank++;
				}
				break;
		}
	}

}

