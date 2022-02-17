
using Sandbox;

internal class MapStats
{

	public int Falls { get; set; }
	public int Respawns { get; set; }
	public int Attempts { get; set; }
	public int Completions { get; set; }
	public float BestTime { get; set; }
	public float TimePlayed { get; set; }

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

	public void AddAttempt()
	{
		Attempts++;
		LocalCookie = ToJson();
	}

	public void AddCompletion()
	{
		if( Completions == 0 )
		{
			Event.Run( "mapstats.firstcompletion" );
		}

		Completions++;
		LocalCookie = ToJson();
	}

	public void SetBestTime( float newTime )
	{
		if ( BestTime != default && BestTime < newTime ) return;
		BestTime = newTime;
		LocalCookie = ToJson();
	}

	public void AddTimePlayed( float seconds )
	{
		TimePlayed += seconds;
		LocalCookie = ToJson();

		Event.Run( "mapstats.ontimeplayed", TimePlayed );
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
