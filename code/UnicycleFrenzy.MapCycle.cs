using Sandbox;
using System.Collections.Generic;
using System.Linq;

internal partial class UnicycleFrenzy
{

	[Net]
	public string NextMap { get; set; }
	[Net]
	public IDictionary<long, string> MapVotes { get; set; }
	[Net]
	public List<string> MapOptions { get; set; }

	private async void InitMapCycle()
	{

		Host.AssertServer();

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

		//NextMap = Rand.FromArray( MapCycle.Where( x => x != Global.MapName ).ToArray() );
	}

	[Event.Tick.Server]
	private void OnTick()
	{
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

		//ChangeMapInternal( mapident );
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
