using Facepunch.Customization;
using Sandbox;
using System.Linq;
using System.Collections.Generic;

partial class UnicycleFrenzy : Sandbox.Game
{
	[Net] public int BotGeneration { get; set; } = 1;

	public UnicyclePlayer BotLeader { get; set; } = null;

	[Net] public Vector3 SpectatePos { get; set; } = new Vector3();
	[Net] public Vector3 SpectateLook { get; set; } = Vector3.Forward;
	public const float MoveSpeed = 2.0f;

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

			//InitMapCycle();
			//_ = GameLoopAsync();
		}
	}

	private void UpdateSpectatorCamera()
	{
		if ( !IsServer )
			return;

		var bots = Client.All.Where( x => x.IsBot && x.Pawn is UnicyclePlayer && x.Pawn.LifeState == LifeState.Alive ).Select( x => x.Pawn as UnicyclePlayer ).OrderByDescending( x => x.BotFitness ).ToArray();

		Vector3 avgPos = new Vector3();
		//Vector3 avgLook = new Vector3();
		float totalWeight = 0.0f;
		for ( int i = 0; i < bots.Length; i++ )
		{
			//float weight = 1.0f - ((float)i / bots.Length) * 0.5f;
			float weight = 1.0f - ((float)i / bots.Length);
			weight = weight * weight;
			avgPos += bots[i].Position * weight;
			//avgLook += bots[i].Rotation.Forward.WithZ(0).Normal * weight;
			//avgLook += FitnessPath.Current.GetDirectionVector(bots[i].Position).WithZ(0).Normal;
			totalWeight += weight;
		}


		if ( totalWeight > 0.0f )
		{
			avgPos /= totalWeight;
			//avgLook /= totalWeight;
			SpectatePos = Vector3.Lerp( SpectatePos, avgPos, Time.Delta * MoveSpeed );
			//CurrentLook = Vector3.Lerp( CurrentLook, avgLook.Normal, Time.Delta * moveSpeed ).Normal;
		}

		// paths aren't replicated so we need to do this here
		SpectateLook = FitnessPath.Current.GetDirectionVector( SpectatePos ).WithZ( 0 ).Normal;

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

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		BotLeader = Client.All.Select( x => x.Pawn ).OfType<UnicyclePlayer>().Where( x => x.LifeState == LifeState.Alive ).OrderByDescending( x => x.BotFitness ).FirstOrDefault();

		if ( IsServer )
			UpdateSpectatorCamera();
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
