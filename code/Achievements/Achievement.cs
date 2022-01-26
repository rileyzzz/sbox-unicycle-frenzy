using System;
using System.Collections.Generic;
using System.Linq;

internal class Achievement
{

	public long AchievementId { get; set; }
	public string GameName { get; set; }
	// we'd use these in api instead of game/map name
	//public long GameAssetId { get; set; } 
	//public long MapAssetId { get; set; }
	public string ShortName { get; set; }
	public string DisplayName { get; set; }
	public string Description { get; set; }
	public string ImageThumb { get; set; }

	public void Set( long playerid )
	{
		// later: push to api
		if ( IsCompleted( playerid ) ) return;

		AchievementCompletion.Insert( playerid, AchievementId );
	}

	public bool IsCompleted( long playerid )
	{
		return AchievementCompletion.Query( playerid, Global.GameName ).Any( x => x.AchievementId == AchievementId );
	}

	public static IEnumerable<Achievement> Query( string game )
	{
		// later: fetch from api
		return All.Where( x => x.GameName == game );
	}

	public static List<Achievement> All
	{
		get
		{
			// later: fetch from api
			var result = new List<Achievement>();

			result.Add( new Achievement()
			{
				AchievementId = 1,
				Description = "A dummy achievement",
				DisplayName = "Dummy",
				ShortName = "uf_dummy",
				GameName = Global.GameName,
				ImageThumb = "https://files.facepunch.com/crayz/1b2511b1/msedge_2022-01-25_17-06-28.png"
			} );

			return result;
		}
	}

}
