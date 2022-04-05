using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

class BotManager
{
	static int generation = 1;

	static List<UnicycleBot> Bots = new();

	[ServerCmd("nn_start")]
	public static void Init()
	{
		for ( int i = 0; i < 48; i++ )
			Bots.Add(UnicycleBot.Create());
	}

	static float Lerp( float a, float b, float f )
	{
		return a + f * (b - a);
	}

	static double Lerp( double a, double b, float f )
	{
		return a + f * (b - a);
	}

	public const int NumInputs = 14;
	public const int NumOutputs = 5;
	public const int LayerSize = 10;
	public const int NumLayers = 1;

	public static UnicycleBrain CreateBrain()
	{
		return new UnicycleBrain( NumInputs, NumOutputs, NumLayers, LayerSize );
	}

	[Event.Tick]
	public static void Update()
	{
		if ( !Host.IsServer || Bots.Count == 0 )
			return;
		//Log.Info("bot update");

		bool finished = Bots.All( x => x.Client.Pawn != null && x.Client.Pawn.LifeState == LifeState.Dead );
		if (finished)
		{
			//UnicycleBot best = Bots[0];
			//float bestFitness = Bots[0].GetFitness();

			var sorted = Bots.OrderByDescending(x => x.GetFitness()).ToList();

			foreach ( var bot in sorted )
			{
				Log.Info( $"{bot.Client.Name} fitness = {bot.GetFitness()}" );
			}

			foreach (var bot in Bots)
			{
				if ( bot.Client.Pawn == null || bot.Client.Pawn is not Player player )
					continue;

				//bot.Fitness
				//float fitness = bot.GetFitness();
				//if ( fitness > bestFitness)
				//{
				//	bestFitness = fitness;
				//	best = bot;
				//}

				//Log.Info($"{bot.Client.Name} fitness = {bot.GetFitness()}");
				bot.Reset();
				player.Respawn();
			}
			if ( !FileSystem.Data.DirectoryExists( "output" ) )
				FileSystem.Data.CreateDirectory("output");
			FileSystem.Data.WriteJson( $"output/gen_{generation}.json", sorted[0].Brain );
			
			const int elitism = 6;
			const int newRandom = 3;

			var newBrains = new List<UnicycleBrain>();

			for (int i = 0; i < newRandom; i++ )
			{
				newBrains.Add(CreateBrain());
			}
			//for ( int i = 0; i < elitism; i++ )
			//{
			//	for ( int j = 0; j < elitism; j++ )
			//	{
			//		// no inbreeding thank you
			//		if ( i == j )
			//			continue;
			//		newBrains.Add(UnicycleBrain.Cross(sorted[i].Brain, sorted[j].Brain));
			//	}
			//}

			int max = 0;
			while (newBrains.Count < Bots.Count)
			{
				if ( ++max >= Bots.Count - 1 )
					max = 0;
				for ( int i = 0; i < max; i++ )
				{
					newBrains.Add( UnicycleBrain.Cross( sorted[i].Brain, sorted[max].Brain ) );
					if ( newBrains.Count >= Bots.Count )
						break;
				}

				//for ( int i = 0; i < elitism; i++ )
				//{
				//	for ( int j = 0; j < elitism; j++ )
				//	{
				//		// no inbreeding thank you
				//		if ( i == j )
				//			continue;
				//		newBrains.Add( UnicycleBrain.Cross( sorted[i].Brain, sorted[j].Brain ) );

				//		if ( newBrains.Count >= Bots.Count )
				//			goto done;
				//	}
				//}
			}

			// fuck with the weights a bit, mutate our beautiful creature
			//const float mutationScale = 0.08f;
			// increase for each bot, have some that play it safe and some that get wacky with it
			float mutationScale = 0.1f;
			//float mutationStep = 0.12f;
			float mutationChance = 0.02f;
			foreach ( var brain in newBrains )
				brain.Mutate( mutationScale, mutationChance );

			int brainIdx = 0;
			foreach (var bot in Bots)
			{
				// you're good, keep doin' what you're doin'
				if ( sorted.IndexOf( bot ) < elitism )
					continue;

				//if ( bot == sorted[0] )
					//continue;

				bot.Brain = newBrains[brainIdx++];
			}
			// create variations of the best of that generation, keep the best the same
			//foreach (var bot in Bots)
			//{
			//	// you're good, keep doin' what you're doin'
			//	if ( bot == best )
			//		continue;

			//	//bot.Brain = best.Brain.Clone();
			//	bot.Brain = UnicycleBrain.Cross(best.Brain, bot.Brain);
			//	bot.Brain.Mutate( mutationScale, mutationChance );

			//	//mutationScale += mutationStep;
			//}
			Log.Info($"Finished generation {generation}.");
			generation++;
		}
	}
}
