using Sandbox;
using System.Collections.Generic;
using System.Linq;

internal partial class UnicycleFrenzy
{

	public static List<string> MapCycle = new()
	{
		"facepunch.uf_splinter",
		"saandy.uf_fort",
		"facepunch.uf_beach",
		"facepunch.uf_flow",
		"facepunch.uf_wooden_gaps",
		"barrelzzen.uf_industry",
		"facepunch.uf_pop_rock",
		"gvar.uf_canyon",
		"facepunch.uf_steps",
		"facepunch.uf_hop_stop"
	};

	[ConVar.Replicated( "uf_endgame_duration" )]
	public static float EndGameDuration { get; set; } = 60 * 1.5f;

	[Net]
	public float TimeLeft { get; set; }
	[Net]
	public string NextMap { get; set; }
	[Net]
	public Dictionary<long, string> MapVotes { get; set; } = new();

	private void InitMapCycle()
	{
		TimeLeft = 1800;
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
