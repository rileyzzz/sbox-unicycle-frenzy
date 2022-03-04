using Sandbox;
using Sandbox.UI;
using System.Linq;

[UseTemplate]
internal class StatsAchievements : NavigatorPanel
{

    public Panel AchievementCanvas { get; set; }
    public string AchievementCount { get; set; }
    public string AchievementName { get; set; }
    public string AchievementDescription { get; set; }
    public StatsAchievements()
    {

    }

    [Event.Entity.PostSpawn]
    private void RebuildAchievements()
    {
        AchievementCanvas.DeleteChildren(true);

        var mapAchievements = Achievement.FetchForMap();
        var total = 0;
        var achieved = 0;

        foreach (var ach in mapAchievements)
        {
            if (!ShowAchievement(ach)) continue;

			if ( IsMedal( ach ) )
			{
				ach.Description = GetMedalDescription( ach );
			}

			var entry = new StatsAchievementsEntry( ach );
			entry.Parent = AchievementCanvas;

			if( GrantsXp(ach) )
			{
				// this achievement is tied to trail pass and will reward xp on completion
			}

            if ( ach.IsCompleted() )
            {
                achieved++;
            }

            total++;
        }

        AchievementCount = $"({achieved}/{total})";
    }

	public override void OnHotloaded() => RebuildAchievements();
	protected override void PostTemplateApplied() => RebuildAchievements();

    private bool ShowAchievement(Achievement ach)
    {
        if (IsMedal(ach) && !Entity.All.Any(x => x is AchievementMedals))
            return false;

        return true;
    }

    private static bool IsMedal(Achievement ach)
    {
        return new string[]
        {
            "uf_bronze",
            "uf_silver",
            "uf_gold"
        }.Contains(ach.ShortName);
    }

    private static bool GrantsXp(Achievement ach)
    {
        return TrailPass.Current.Achievements.Any(x => x.AchievementShortName == ach.ShortName);
    }

    private static string GetMedalDescription(Achievement ach)
    {
        var achMedals = Entity.All.FirstOrDefault(x => x is AchievementMedals) as AchievementMedals;
        if (!achMedals.IsValid()) return ach.Description;

        var time = ach.ShortName switch
        {
            "uf_bronze" => achMedals.Bronze,
            "uf_silver" => achMedals.Silver,
            "uf_gold" => achMedals.Gold,
            _ => 0
        };

        return $"Complete the map in {CourseTimer.FormattedTimeMs(time)}s or better";
    }

}

