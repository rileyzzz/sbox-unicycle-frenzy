using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sandbox;

class BotManager
{
	public static int Generation = 1;

	public static List<UnicycleBot> Bots = new();

	public static UnicycleAI.NetworkModel Model;

	static bool Resetting = false;
	static bool ResetFinished = false;
	static bool Initialized = false;

	public static Client Leader
	{
		get
		{
			var bots = Client.All.Where( x => x.IsBot && x.Pawn is UnicyclePlayer && x.Pawn.LifeState == LifeState.Alive )
				//.Select( x => x.Pawn as UnicyclePlayer )
				.OrderByDescending( x => ((UnicyclePlayer)x.Pawn).BotFitness );
			return bots.FirstOrDefault();
		}
	}


	//public const int NumInputs = 16;
	public const int NumInputs = 2 + (UnicycleBot.VisionRange + 1) * (UnicycleBot.VisionRange + 1);
	//public const int NumInputs = 2;
	public const int NumOutputs = 4;
	//public const int LayerSize = 12;
	public const int LayerSize = 40;
	public const int NumLayers = 1;
	[ServerCmd("nn_start")]
	public static void Init()
	{
		//Model = new UnicycleAI.PerceptronModel();
		Model = new UnicycleAI.NEATModel();

		var agents = new List<UnicycleAI.NetworkAgent>();
		for ( int i = 0; i < 25; i++ )
		{
			var bot = UnicycleBot.Create();
			Bots.Add( bot );
			agents.Add( bot );
		}

		// nice one, facepunch
		// Task`1.* is whitelisted but not Task.*
		//_ = Model.Init(agents, NumInputs, NumOutputs).ContinueWith(e => {
		//	Initialized = true;
		//} );
		_ = InitInternal(agents);

		//_ = Model.StartGeneration();
	}

	private static async Task InitInternal( List<UnicycleAI.NetworkAgent> agents )
	{
		await Model.Init( agents, NumInputs, NumOutputs );
		Initialized = true;
	}

	static float Lerp( float a, float b, float f )
	{
		return a + f * (b - a);
	}

	static double Lerp( double a, double b, float f )
	{
		return a + f * (b - a);
	}



	//public static UnicycleAI.UnicycleBrain CreateBrain()
	//{
	//	//return new UnicycleAI.UnicycleBrain( NumInputs, NumOutputs, NumLayers, LayerSize );
	//	return new UnicycleAI.PerceptronBrain( NumInputs, NumOutputs, NumLayers, LayerSize );
	//}

	[Event.Tick]
	public static void Update()
	{
		if ( !Host.IsServer || Bots.Count == 0 )
			return;
		//Log.Info("bot update");

		if ( !Initialized )
			return;

		bool finished = Bots.All( x => x.Client.Pawn != null && x.Client.Pawn.LifeState == LifeState.Dead );
		if ( finished && !Resetting )
		{
			Resetting = true;
			ResetFinished = false;

			_ = FinishGeneration();
		}
		
		if (ResetFinished)
		{
			Resetting = false;
			ResetFinished = false;
			Log.Info( $"Finished generation {Generation}." );
			Generation++;
			UnicycleFrenzy.Game.BotGeneration = Generation;

			foreach ( var bot in Bots )
			{
				if ( bot.Client.Pawn == null || bot.Client.Pawn is not Player player )
					continue;

				bot.Reset();
				player.Respawn();
			}

			Model.StartGeneration();
		}
	}

	static async Task FinishGeneration()
	{
		Log.Info("Finishing generation...");
		await Model.FinishGeneration();

		ResetFinished = true;
	}
}
