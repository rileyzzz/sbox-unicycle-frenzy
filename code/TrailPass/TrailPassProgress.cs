using System.Collections.Generic;
using System.Text.Json;

internal class TrailPassProgress
{

	public int TrailPassId { get; set; }
	public int Experience { get; set; }
	public List<int> UnlockedItems { get; set; } = new();


	public static TrailPassProgress CurrentSeason => Deserialize( Cookie.Get( SeasonCookie, "" ) );
	public bool Unlocked( int id ) => UnlockedItems.Contains( id );
	public void Save() => Cookie.Set( SeasonCookie, Serialize( this ) );

	private static string SeasonCookie => "uf.trailpass." + TrailPass.Season;

	private static TrailPassProgress Deserialize( string json )
	{
		try
		{
			return JsonSerializer.Deserialize<TrailPassProgress>( json );
		}
		catch( System.Exception e )
		{
			Log.Error( e.Message );
		}
		return new() { TrailPassId = TrailPass.Season };
	}

	private static string Serialize( TrailPassProgress ticket )
	{
		return JsonSerializer.Serialize( ticket );
	}

}
