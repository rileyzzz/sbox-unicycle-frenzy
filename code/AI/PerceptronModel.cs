using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Sandbox;

namespace UnicycleAI
{
	public abstract class PerceptronModel : NetworkModel
	{
		public PerceptronModel()
		{
		}

		public override async Task Init( List<NetworkAgent> agents, int numInputs, int numOutputs )
		{
			await base.Init( agents, numInputs, numOutputs );

		}
		public override void StartGeneration()
		{

		}

		public override async Task FinishGeneration()
		{
			var sorted = Agents.OrderByDescending( x => x.GetFitness() ).ToList();

			////foreach ( var bot in sorted )
			////{
			////	Log.Info( $"{bot.Client.Name} fitness = {bot.GetFitness()}" );
			////}

			////if ( !FileSystem.Data.DirectoryExists( "output" ) )
			////	FileSystem.Data.CreateDirectory( "output" );
			////FileSystem.Data.WriteJson( $"output/gen_{Generation}.json", sorted[0].Brain );

			//const int elitism = 6;
			//const int newRandom = 3;

			//var newBrains = new List<UnicycleAI.UnicycleBrain>();

			//for ( int i = 0; i < newRandom; i++ )
			//{
			//	newBrains.Add( CreateBrain() );
			//}

			//int max = 0;
			//while ( newBrains.Count < Bots.Count )
			//{
			//	if ( ++max >= Bots.Count - 1 )
			//		max = 0;
			//	for ( int i = 0; i < max; i++ )
			//	{
			//		newBrains.Add( sorted[i].Brain.Cross( sorted[max].Brain ) );
			//		if ( newBrains.Count >= Bots.Count )
			//			break;
			//	}

			//	//for ( int i = 0; i < elitism; i++ )
			//	//{
			//	//	for ( int j = 0; j < elitism; j++ )
			//	//	{
			//	//		// no inbreeding thank you
			//	//		if ( i == j )
			//	//			continue;
			//	//		newBrains.Add( UnicycleBrain.Cross( sorted[i].Brain, sorted[j].Brain ) );

			//	//		if ( newBrains.Count >= Bots.Count )
			//	//			goto done;
			//	//	}
			//	//}
			//}

			//// fuck with the weights a bit, mutate our beautiful creature
			////const float mutationScale = 0.08f;
			//// increase for each bot, have some that play it safe and some that get wacky with it
			//float mutationScale = 0.3f;
			////float mutationStep = 0.12f;
			//float mutationChance = 0.01f;
			//foreach ( var brain in newBrains )
			//	brain.Mutate( mutationScale, mutationChance );

			//int brainIdx = 0;
			//foreach ( var bot in Bots )
			//{
			//	// you're good, keep doin' what you're doin'
			//	if ( sorted.IndexOf( bot ) < elitism )
			//		continue;

			//	//if ( bot == sorted[0] )
			//	//continue;

			//	bot.Brain = newBrains[brainIdx++];
			//}
			//// create variations of the best of that generation, keep the best the same
			////foreach (var bot in Bots)
			////{
			////	// you're good, keep doin' what you're doin'
			////	if ( bot == best )
			////		continue;

			////	//bot.Brain = best.Brain.Clone();
			////	bot.Brain = UnicycleBrain.Cross(best.Brain, bot.Brain);
			////	bot.Brain.Mutate( mutationScale, mutationChance );

			////	//mutationScale += mutationStep;
			////}
		}
	}
}
