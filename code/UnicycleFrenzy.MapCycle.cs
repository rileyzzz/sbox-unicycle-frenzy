using Sandbox;
using System.Collections.Generic;
using System.Linq;

internal partial class UnicycleFrenzy
{

	[ConVar.Replicated( "uf_endgame_duration" )]
	public static float EndGameDuration { get; set; } = 60 * 1.5f;

	public const float GameDuration = 30 * 60f; // 30 mins

	[Net]
	public RealTimeUntil GameTimer { get; set; }
	[Net]
	public string NextMap { get; set; }
	[Net]
	public Dictionary<long, string> MapVotes { get; set; }
	[Net]
	public List<string> MapOptions { get; set; }

	private async void InitMapCycle()
	{
		Host.AssertServer();

		GameTimer = GameDuration;
		NextMap = Global.MapName;

		var pkg = await Package.Fetch( Global.GameIdent, true );
		if ( pkg == null )
		{
			Log.Error( "Failed to load map cycle" );
			return;
		}

		var maps = pkg.GetMeta<List<string>>( "MapList" )
			.Where( x => x != Global.MapName )
			.OrderBy( x => Rand.Int( 99999 ) )
			.Take( 5 )
			.ToList();

		MapOptions = maps;
		NextMap = Rand.FromList( maps );
	}

	[Event.Tick.Server]
	private void OnTick()
	{
		if ( GameTimer > 0 ) return;

		ServerCmd_ChangeMap( NextMap );
	}

	[ServerCmd]
	public static void ServerCmd_ChangeMap( string mapident )
	{
		Global.ChangeLevel( mapident );
	}

	[ServerCmd]
	public static void ServerCmd_SetMapVote( string mapIdent )
	{
		if ( !ConsoleSystem.Caller.IsValid() ) 
			return;

		if ( Game.MapVotes.TryGetValue( ConsoleSystem.Caller.PlayerId, out var vote ) && vote == mapIdent )
			return;

		Game.MapVotes[ConsoleSystem.Caller.PlayerId] = mapIdent;
		Game.NextMap = Game.MapVotes.OrderByDescending( x => x.Value ).First().Value;

		UfChatbox.AddInfo( To.Everyone, string.Format( "{0} voted for {1}", ConsoleSystem.Caller.Name, mapIdent ) );
	}

}
