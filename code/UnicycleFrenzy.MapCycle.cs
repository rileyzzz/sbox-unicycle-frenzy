using Sandbox;
using System.Collections.Generic;
using System.Linq;

internal partial class UnicycleFrenzy
{

	//
	// NOTE: Add new maps in the in-game setup menu
	//

	[ConVar.Replicated( "uf_endgame_duration" )]
	public static float EndGameDuration { get; set; } = 60 * 1.5f;

	[Net]
	public float TimeLeft { get; set; }
	[Net]
	public string NextMap { get; set; }
	[Net]
	public Dictionary<long, string> MapVotes { get; set; } = new();
	[Net]
	public List<string> MapCycle { get; set; } = new();

	private async void InitMapCycle()
	{
		//TimeLeft = 1800;
		TimeLeft = 0;
		NextMap = Global.MapName;

		var pkg = await Package.Fetch( Global.GameIdent, true );
		if ( pkg == null )
		{
			Log.Error( "Failed to load map cycle" );
			return;
		}

		var availablemaps = pkg.GetMeta<List<string>>( "MapList" );
		availablemaps.RemoveAll( x => x == Global.MapName );

		for ( int i = 0; i < 5; i++ )
		{
			var chosen = Rand.FromList( availablemaps );
			availablemaps.Remove( chosen );
			MapCycle.Add( chosen );
		}

		NextMap = Rand.FromArray( MapCycle.Where( x => x != Global.MapName ).ToArray() );
	}

	[Event.Tick.Server]
	private void OnTick()
	{
		TimeLeft += Time.Delta;
		//if ( TimeLeft > 0 )
		//{
		//	TimeLeft -= Time.Delta;

		//	if ( TimeLeft <= 0 )
		//	{
		//		ChangeMapInternal( NextMap );
		//	}
		//}
	}

	public void ChangeMap( string mapident )
	{
		if ( !IsServer && !Global.IsListenServer ) return;

		ChangeMapInternal( mapident );
	}

	[ServerCmd]
	private static void ChangeMapInternal( string mapident )
	{
		Global.ChangeLevel( mapident );
	}

	[ServerCmd]
	public static void SetMapVote( string mapIdent )
	{
		if ( !ConsoleSystem.Caller.IsValid() ) return;

		if(Game.MapVotes.ContainsKey(ConsoleSystem.Caller.PlayerId)
			&& Game.MapVotes[ConsoleSystem.Caller.PlayerId] == mapIdent )
		{
			return;
		}

		Game.MapVotes[ConsoleSystem.Caller.PlayerId] = mapIdent;

		var sort = new Dictionary<string, int>();
		foreach( var kvp in Game.MapVotes )
		{
			if ( !sort.ContainsKey( kvp.Value ) )
			{
				sort.Add( kvp.Value, 0 );
			}
			sort[kvp.Value]++;
		}
		Game.NextMap = sort.OrderByDescending( x => x.Value ).First().Key;

		UfChatbox.AddInfo( To.Everyone, string.Format( "{0} voted for {1}", ConsoleSystem.Caller.Name, mapIdent ) );
	}

}
