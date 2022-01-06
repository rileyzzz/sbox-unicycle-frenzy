using Sandbox;
using System.Collections.Generic;
using System.Linq;

internal partial class UnicycleFrenzy
{

	public static List<string> MapCycle = new()
	{
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

}
