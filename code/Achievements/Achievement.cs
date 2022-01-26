using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

internal partial class Achievement
{

	//
	// todo: there's some code confusion with PerMap and MapName
	// there's probably a better way to separate the two, but we'll see 
	// what kinda headache it causes first
	//

	public long AchievementId { get; set; }
	public string GameName { get; set; }
	public string MapName { get; set; }
	// we'd use these in api instead of game/map name
	//public long GameAssetId { get; set; } 
	//public long MapAssetId { get; set; }
	public string ShortName { get; set; }
	public string DisplayName { get; set; }
	public string Description { get; set; }
	public string ImageThumb { get; set; }
	public bool PerMap { get; set; }

	public bool IsCompleted( long playerid, string game, string map = null )
	{
		return AchievementCompletion.Query( playerid, game, map ).Any( x => x.AchievementId == AchievementId );
	}

	public static IEnumerable<Achievement> Query( string game, string shortname = null, string map = null )
	{
		// later: fetch from api
		var result = All.Where( x => x.GameName == game && x.MapName == map );

		if ( !string.IsNullOrEmpty( shortname ) ) 
			result = result.Where( x => x.ShortName == shortname );

		return result;
	}

	public static void Set( string game, long playerid, string shortname, string map = null )
	{
		Host.AssertClient();

		var ach = Query( game, shortname ).FirstOrDefault();

		if ( ach == null ) return;
		if ( ach.MapName != null && ach.MapName != map && !ach.PerMap ) return;
		if ( ach.IsCompleted( playerid, game, map ) ) return;

		var mapToInsert = ach.PerMap ? map : ach.MapName;

		AchievementCompletion.Insert( playerid, ach.AchievementId, mapToInsert );
	}

	public static List<Achievement> All
	{
		get
		{
			// later: fetch from api
			var result = new List<Achievement>();

			result.Add( new Achievement()
			{
				AchievementId = 2,
				Description = "Complete any map in Unicycle Frenzy",
				DisplayName = "Unicyclist",
				ShortName = "uf_unicyclist",
				GameName = Global.GameName,
				ImageThumb = "https://files.facepunch.com/crayz/1b2611b1/icon-unicyclist.jpg"
			} );

			result.Add( new Achievement()
			{
				AchievementId = 3,
				Description = "Complete the map in an ok amount of time",
				DisplayName = "Bronze Medal",
				ShortName = "uf_bronze",
				GameName = Global.GameName,
				ImageThumb = "",
				PerMap = true
			} );

			result.Add( new Achievement()
			{
				AchievementId = 4,
				Description = "Complete the map in a decent amount of time",
				DisplayName = "Silver Medal",
				ShortName = "uf_silver",
				GameName = Global.GameName,
				ImageThumb = "",
				PerMap = true
			} );

			result.Add( new Achievement()
			{
				AchievementId = 5,
				Description = "Complete the map in a good amount of time",
				DisplayName = "Bronze Medal",
				ShortName = "uf_gold",
				GameName = Global.GameName,
				ImageThumb = "",
				PerMap = true
			} );

			result.Add( new Achievement()
			{
				AchievementId = 6,
				Description = "An achievement specifically for uf_playground",
				DisplayName = "Playground",
				ShortName = "uf_playground_test",
				GameName = Global.GameName,
				MapName = "uf_playground",
				ImageThumb = ""
			} );

			return result;
		}
	}

}
