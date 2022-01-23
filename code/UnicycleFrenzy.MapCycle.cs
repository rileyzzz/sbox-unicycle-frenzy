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
		TimeLeft = 1800;
		NextMap = Global.MapName;

		var pkg = await Package.Fetch( Global.GameName, true );
		if( pkg == null )
		{
			Log.Error( "Failed to load map cycle" );
			return;
		}

		foreach( var map in pkg.GameConfiguration.MapList )
			MapCycle.Add( map );

		NextMap = Rand.FromArray( MapCycle.Where( x => x != Global.MapName ).ToArray() );
	}

	[Event.Tick.Server]
	private void OnTick()
	{
		if ( TimeLeft > 0 )
		{
			TimeLeft -= Time.Delta;

			if ( TimeLeft <= 0 )
			{
				Global.ChangeLevel( NextMap );
			}
		}
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
