using Sandbox;

internal partial class UnicyclePlayer
{

	public void AddTrailPassExperience( int amount )
	{
		Host.AssertClient();

		var progress = TrailPassProgress.CurrentSeason;
		progress.Experience += amount;
		progress.Save();
	}

	[Event("mapstats.firstcompletion")]
	public void OnFirstCompletion()
	{
		AddTrailPassExperience( 50 );
	}

	[Event("achievement.set")]
	public void OnAchievementSet( string shortname )
	{
		Host.AssertClient();

		switch ( shortname )
		{
			case "uf_bronze":
				AddTrailPassExperience( 30 );
				break;
			case "uf_silver":
				AddTrailPassExperience( 40 );
				break;
			case "uf_gold":
				AddTrailPassExperience( 50 );
				break;
		}
	}

}
