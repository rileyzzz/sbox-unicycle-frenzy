using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;

internal class AchievementCompletion
{

	public long AchievementId { get; set; } // key
	public long SteamId { get; set; } // key
	// public long MapAssetId { get; set; } // key
	public string MapName { get; set; }
	public DateTimeOffset Time { get; set; }

	public static List<AchievementCompletion> Query( long playerid, string game, string map = null )
	{
		// later: fetch from api
		var result = new List<AchievementCompletion>();
		var all = All.Where( x => x.SteamId == playerid );
		if ( map != null ) all = All.Where( x => x.MapName == map );

		foreach( var completion in all )
		{
			var ach = Achievement.All.FirstOrDefault( x => x.AchievementId == completion.AchievementId );
			if ( ach == null ) continue;
			if ( ach.GameName != game ) continue;
			if ( ach.MapName != map && !ach.PerMap ) continue;
			result.Add( completion );
		}

		return result;
	}

	public static void Insert( long playerid, long achievementid, string map = null )
	{
		var final = new List<AchievementCompletion>( All )
		{
			new AchievementCompletion()
			{
				AchievementId = achievementid,
				SteamId = playerid,
				MapName = map,
				Time = DateTime.Now
			}
		};

		Cookie.Set( "uf.achievement_completions", JsonSerializer.Serialize( final ) );
	}

	public static List<AchievementCompletion> All
	{
		get
		{
			try
			{
				Log.Info( Cookie.Get( "uf.achievement_completions", "{}" ) );
				return JsonSerializer.Deserialize<List<AchievementCompletion>>( Cookie.Get( "uf.achievement_completions", "{}" ) );
			}
			catch(Exception e )
			{
				Log.Error( e );
				return new List<AchievementCompletion>();
			}
		}
	}

}
