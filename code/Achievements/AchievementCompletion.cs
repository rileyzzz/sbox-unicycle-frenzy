using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;

internal class AchievementCompletion
{

	public long AchievementId { get; set; } // key
	public long SteamId { get; set; } // key
	public DateTimeOffset Time { get; set; }

	public static List<AchievementCompletion> Query( long playerid, string game )
	{
		// later: fetch from api
		var result = new List<AchievementCompletion>();

		foreach( var completion in All.Where( x => x.SteamId == playerid ) )
		{
			var ach = Achievement.All.FirstOrDefault( x => x.AchievementId == completion.AchievementId );
			if ( ach == null ) continue;
			if ( ach.GameName != game ) continue;
			result.Add( completion );
		}

		return result;
	}

	public static void Insert( long playerid, long achievementid )
	{
		var final = new List<AchievementCompletion>( All )
		{
			new AchievementCompletion()
			{
				AchievementId = achievementid,
				SteamId = playerid,
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
