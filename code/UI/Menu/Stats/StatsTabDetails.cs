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

	public override void Tick()
	{
		base.Tick();

		SetClass( "incomplete", Stats.BestTime == 0 );
	}

	public override void OnHotloaded()
	{
		base.OnHotloaded();

		RebuildAchievements();
	}

	protected override void PostTemplateApplied()
	{
		base.PostTemplateApplied();

		RebuildAchievements();
	}

	public void RebuildAchievements()
	{
		AchievementCanvas.DeleteChildren( true );

		var mapAchievements = Achievement.Query( Global.GameName, map: Global.MapName );
		var globalAchievements = Achievement.Query( Global.GameName );

		foreach ( var ach in mapAchievements.Concat( globalAchievements ) )
		{
			var btn = new Button();
			btn.AddClass( "button icon" );
			btn.Parent = AchievementCanvas;
			btn.Style.SetBackgroundImage( ach.ImageThumb );

			var map = ach.PerMap ? Global.MapName : null;

			if ( ach.IsCompleted( Local.PlayerId, Global.GameName, map ) )
			{
				btn.AddClass( "completed" );
			}
		}
	}

}

