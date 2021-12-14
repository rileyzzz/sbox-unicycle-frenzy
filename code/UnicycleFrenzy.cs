using Sandbox;
using System.Collections.Generic;

partial class UnicycleFrenzy : Sandbox.Game
{

	public static UnicycleFrenzy Game => Current as UnicycleFrenzy;

	[Net]
	public float GameTime { get; set; }

	private List<string> mapCycle = new()
	{
		"uf_steps",
		"uf_hop_stop"
	};

	private List<string> fallMessages = new()
	{
		"{0} ate shit 💩",
		"{0} fell ass over tea-kettle",
		"Wow, did you see {0} bail that landing?",
		"{0} just went arse over tit!",
		"{0} adopted a tree this morning!",
		"{0} needs some practice 😂",
		"It's a skill problem for {0} 🤙",
		"{0} must have missed the \"wet floor\" warning"
	};

	public UnicycleFrenzy()
	{
		if ( IsClient )
		{
			new UnicycleHud();
		}

		if ( IsServer )
		{
			GameTime = 1800;
		}

		mapCycle.Remove( Global.MapName.ToLower() );
	}

	public override void ClientJoined( Client cl )
	{
		base.ClientJoined( cl );

		cl.Components.Add( new UnicycleEnsemble() );

		cl.Pawn = new UnicyclePlayer();
		(cl.Pawn as Player).Respawn();
	}

	public override void OnKilled( Client client, Entity pawn )
	{
		base.OnKilled( client, pawn );

		UfChatbox.AddInfo( To.Everyone, GetRandomFallMessage( client.Name ), "Obs" );
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( !IsServer ) return;

		if( GameTime > 0 )
		{
			GameTime -= Time.Delta;

			if ( GameTime <= 0 )
			{
				LoadNextMap();
			}
		}
	}

	private void LoadNextMap()
	{
		Global.ChangeLevel( Rand.FromArray( mapCycle.ToArray() ) );
	}

	private int lastFallMessage;
	private string GetRandomFallMessage( string playerName )
	{
		var idx = Rand.Int( 0, fallMessages.Count - 1 );
		while ( idx == lastFallMessage )
			idx = Rand.Int( 0, fallMessages.Count - 1 );

		lastFallMessage = idx;
		return string.Format( fallMessages[idx], playerName );
	}

}
