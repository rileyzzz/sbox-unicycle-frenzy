using Sandbox;
using Sandbox.UI;
using System.Linq;

[UseTemplate]
[NavigatorTarget("menu/stats/details")]
internal class StatsTabDetails : Panel
{

	public MapStats Stats => MapStats.Local;
	public string BestTime => Stats.BestTime == 0 ? "INCOMPLETE" : CourseTimer.FormattedTimeMsf( Stats.BestTime );
	public string TimePlayed => CourseTimer.FormattedTimeMs( Stats.TimePlayed );
	public string MapName => Global.MapName;
	public Panel Thumbnail { get; set; }
	public Panel AchievementCanvas { get; set; }
	public string AchievementCount { get; set; }
	public string AchievementName { get; set; }
	public string AchievementDescription { get; set; }

	public StatsTabDetails()
	{
		SetThumbnail();
	}

	private async void SetThumbnail()
	{
		// todo: in-game screenshot for map thumb?
		var pgk = await Package.Fetch( Global.MapName, true );
		if ( pgk == null ) return;
		Thumbnail.Style.SetBackgroundImage( pgk.Thumb );
	}

	[Event.Entity.PostSpawn]
	private void RebuildAchievements()
	{
		AchievementCanvas.DeleteChildren( true );

		var mapAchievements = Achievement.Query( Global.GameIdent, map: Global.MapName );
		var globalAchievements = Achievement.Query( Global.GameIdent );
		var total = 0;
		var achieved = 0;

		foreach ( var ach in mapAchievements.Concat( globalAchievements ) )
		{
			if ( !ShowAchievement( ach ) ) continue;

			var btn = new Button();
			btn.AddClass( "button icon" );
			btn.Parent = AchievementCanvas;
			btn.Style.SetBackgroundImage( ach.ImageThumb );

			var map = ach.PerMap ? Global.MapName : null;

			if ( ach.IsCompleted( Local.PlayerId, Global.GameIdent, map ) )
			{
				achieved++;
				btn.AddClass( "completed" );
			}

			btn.AddEventListener( "onmouseover", () =>
			 {
				 AchievementName = ach.DisplayName;
				 AchievementDescription = IsMedal( ach ) ? GetMedalDescription( ach ) : ach.Description;
			 } );

			total++;
		}

		AchievementCount = $"({achieved}/{total})";
	}

	public override void Tick() => SetClass( "incomplete", Stats.BestTime == 0 );
	public override void OnHotloaded() => RebuildAchievements();
	protected override void PostTemplateApplied() => RebuildAchievements();

	private bool ShowAchievement( Achievement ach )
	{
		if ( IsMedal( ach ) && !Entity.All.Any( x => x is AchievementMedals ) ) 
			return false;

		return true;
	}

	private static bool IsMedal( Achievement ach )
	{
		return new string[] 
		{
			"uf_bronze",
			"uf_silver",
			"uf_gold"
		}.Contains( ach.ShortName );
	}

	private static string GetMedalDescription( Achievement ach )
	{
		var achMedals = Entity.All.FirstOrDefault( x => x is AchievementMedals ) as AchievementMedals;
		if ( !achMedals.IsValid() ) return ach.Description;

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
