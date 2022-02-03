using Sandbox;
using Hammer;

[Library("uf_achievement_medals")]
[EntityTool( "Unicycle Frenzy Medals", "Unicycle Frenzy", "Set the time for achievement medals." )]
internal class AchievementMedals : Entity
{

	[Property("gold_threshold", "Time (in seconds) to achieve the gold medal for this map")]
	public float Gold { get; set; }
	[Property( "silver_threshold", "Time (in seconds) to achieve the silver medal for this map" )]
	public float Silver { get; set; }
	[Property( "bronze_threshold", "Time (in seconds) to achieve the bronze medal for this map" )]
	public float Bronze { get; set; }

}
