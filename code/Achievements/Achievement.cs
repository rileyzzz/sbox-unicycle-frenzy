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

	public bool IsCompleted( long playerid, string map = null )
	{
		if( PerMap )
		{
			return AchievementCompletion.Query( playerid, Global.GameName )
				.Any( x => x.AchievementId == AchievementId && x.MapName == map );
		}

		return AchievementCompletion.Query( playerid, Global.GameName )
			.Any( x => x.AchievementId == AchievementId );
	}

	public static IEnumerable<Achievement> Query( string game, string shortname = null, string map = null )
	{
		// later: fetch from api
		var result = All.Where( x => x.GameName == game );

		if ( !string.IsNullOrEmpty( shortname ) ) 
			result = result.Where( x => x.ShortName == shortname );

		if ( !string.IsNullOrEmpty( map ) )
			result = result.Where( x => x.PerMap || x.MapName == map );

		return result;
	}

	public static void Set( string game, long playerid, string shortname, string map = null )
	{
		Host.AssertClient();

		var ach = Query( game, shortname, map ).FirstOrDefault();

		if ( ach == null ) return;
		if ( ach.IsCompleted( playerid, map ) ) return;

		AchievementCompletion.Insert( playerid, ach.AchievementId, map );
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

			return result;
		}
	}

}
