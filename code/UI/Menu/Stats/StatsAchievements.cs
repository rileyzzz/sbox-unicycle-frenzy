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

        var mapAchievements = Achievement.Query(Global.GameIdent, map: Global.MapName);
        var globalAchievements = Achievement.Query(Global.GameIdent);
        var total = 0;
        var achieved = 0;

        foreach (var ach in mapAchievements.Concat(globalAchievements))
        {
            if (!ShowAchievement(ach)) continue;

            var btn = new Button();
            btn.AddClass("button icon");
            btn.SetClass("grants-xp", GrantsXp(ach));
            btn.Add.Panel("grayscale");
            btn.Style.SetBackgroundImage(ach.ImageThumb);
            btn.Parent = AchievementCanvas;

            var map = ach.PerMap ? Global.MapName : null;

            if (ach.IsCompleted(Local.PlayerId, Global.GameIdent, map))
            {
                achieved++;
                btn.AddClass("completed");
            }

            btn.AddEventListener("onmouseover", () =>
           {
               AchievementName = GetAchievementTitle(ach);
               AchievementDescription = IsMedal(ach) ? GetMedalDescription(ach) : ach.Description;
           });

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

    private static string GetAchievementTitle(Achievement ach)
    {
        var pass = TrailPass.Current;

        var tpAchi = pass.Achievements.FirstOrDefault(x => x.AchievementShortName == ach.ShortName);
        if (tpAchi == null) return ach.DisplayName;

        return $"{ach.DisplayName} (+{tpAchi.ExperienceGranted}xp)";
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

