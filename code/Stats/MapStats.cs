
internal class MapStats
{

	public int Falls { get; set; }
	public int Respawns { get; set; }
	public int TimesPlayed { get; set; }
	public float BestTime { get; set; }

	public void AddFall()
	{
		Falls++;
		LocalCookie = ToJson();
	}

	public void AddRespawn()
	{
		Respawns++;
		LocalCookie = ToJson();
	}

	public void AddTimesPlayed()
	{
		TimesPlayed++;
		LocalCookie = ToJson();
	}

	public void SetBestTime( float newTime )
	{
		if ( BestTime != default && BestTime < newTime ) return;
		BestTime = newTime;
		LocalCookie = ToJson();
	}

	private string ToJson()
	{
		return System.Text.Json.JsonSerializer.Serialize( this );
	}

	public static string LocalCookie
	{
		get => Cookie.Get( "unicycle.stats." + Global.MapName, new MapStats().ToJson() );
		set => Cookie.Set( "unicycle.stats." + Global.MapName, value );
	}

	public static MapStats Local => System.Text.Json.JsonSerializer.Deserialize<MapStats>( LocalCookie );

}
