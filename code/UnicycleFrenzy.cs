﻿using Facepunch.Customization;
using Sandbox;
using System.Collections.Generic;

partial class UnicycleFrenzy : Sandbox.Game
{

	public static UnicycleFrenzy Game => Current as UnicycleFrenzy;

	private List<string> fallMessages = new()
	{
		"{0} ate shit 💩",
		"{0} fell ass over tea-kettle",
		"Wow, did you see {0} bail that landing?",
		"{0} just went arse over tit!",
		"{0} adopted a tree this morning!",
		"{0} needs some practice 😂",
		"It's a skill problem for {0} 🤙",
		"{0} must have missed the \"wet floor\" warning",
		"{0} had an oopsy!",
		"That wasn't insane {0}",
		"{0} lost the plot!"
	};

	public UnicycleFrenzy()
	{
		if ( IsClient )
		{
			new UnicycleHud();
		}

		if ( IsServer )
		{
			foreach( var part in Customization.Config.Parts )
			{
				Precache.Add( part.AssetPath );
			}

<<<<<<< HEAD
			//BotManager.Init();
=======
			//InitMapCycle();
			_ = GameLoopAsync();
>>>>>>> 4846b82009dbeb1080bd32b7f034aef2be3c98ae
		}
	}

	public override void ClientJoined( Client cl )
	{
		base.ClientJoined( cl );

		if (!cl.IsBot)
		{
			cl.Pawn = new SpectatorPlayer();
			(cl.Pawn as Player).Respawn();
		}
		else
		{
			cl.Components.Add( new CustomizationComponent() );

			cl.Pawn = new UnicyclePlayer();
			(cl.Pawn as Player).Respawn();

			//if ( cl.IsBot )
			//{
			//	(cl.Pawn as UnicyclePlayer).BestTime = new System.Random().Next( 180, 1800 );
			//}
		}


		UfChatbox.AddInfo( To.Everyone, $"{cl.Name} has joined the game" );
	}

	public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( cl, reason );

		UfChatbox.AddInfo( To.Everyone, $"{cl.Name} has left the game" );
	}

	public override void OnKilled( Client client, Entity pawn )
	{
		base.OnKilled( client, pawn );

		UfKillfeed.AddEntryOnClient( To.Everyone, GetRandomFallMessage( client.Name ), client.NetworkIdent );
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

	private float secondCounter = 0;
	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		secondCounter += Time.Delta;

		if ( secondCounter > 1f )
		{
			MapStats.Local.AddTimePlayed( secondCounter );
			secondCounter = 0;
		}
	}

	public System.Action CustomizationChanged;
	private TimeSince timeSinceDirtyCheck;

	[Event.Tick]
	private async void Tempcustmoziationhotload()
	{
		if ( timeSinceDirtyCheck < 1f ) return;
		timeSinceDirtyCheck = 0;

		//todo: FileSystem.Watcher so we can dodge this bs
		if ( await Customization.IsDirty() )
		{
			await Customization.LoadConfig();
			CustomizationChanged?.Invoke();
		}
	}

}
