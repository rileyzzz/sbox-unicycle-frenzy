using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Sandbox;
using SharpNeat.Core;
using SharpNeat.Decoders;
using SharpNeat.Decoders.Neat;
using SharpNeat.DistanceMetrics;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.EvolutionAlgorithms.ComplexityRegulation;
using SharpNeat.Genomes.Neat;
using SharpNeat.Network;
using SharpNeat.Phenomes;
using SharpNeat.SpeciationStrategies;

namespace UnicycleAI
{
	public class NEATModel : NetworkModel
	{
		NeatEvolutionAlgorithmParameters eaParams;
		NeatGenomeParameters neatGenomeParams;
		NetworkActivationScheme activationScheme;
		NeatEvolutionAlgorithm<NeatGenome> Algorithm;
		UnicycleBlackBoxEvaluator Evaluator;
		IGenomeListEvaluator<NeatGenome> SelectiveEvaluator;
		IGenomeFactory<NeatGenome> GenomeFactory;
		List<NeatGenome> GenomeList;

		public NEATModel()
		{
		}

		public const ComplexityCeilingType ceilingType = ComplexityCeilingType.Absolute;
		public const double complexityThreshold = 10.0;


		public override async Task Init( List<NetworkAgent> agents, int numInputs, int numOutputs )
		{
			await base.Init( agents, numInputs, numOutputs );

			//activationScheme = NetworkActivationScheme.CreateAcyclicScheme();
			activationScheme = NetworkActivationScheme.CreateCyclicFixedTimestepsScheme(1);

			eaParams = new NeatEvolutionAlgorithmParameters();
			eaParams.SpecieCount = 10;

			neatGenomeParams = new NeatGenomeParameters();
			//neatGenomeParams.FeedforwardOnly = _activationScheme.AcyclicNetwork;
			//neatGenomeParams.FeedforwardOnly = true;
			//neatGenomeParams.ActivationFn = LeakyReLU.__DefaultInstance;

			// Create a genome factory with our neat genome parameters object and the appropriate number of input and output neuron genes.
			GenomeFactory = new NeatGenomeFactory( numInputs, numOutputs, neatGenomeParams );

			// Create an initial population of randomly generated genomes.
			GenomeList = GenomeFactory.CreateGenomeList( Agents.Count, 0 );

			// Create distance metric. Mismatched genes have a fixed distance of 10; for matched genes the distance is their weight difference.
			IDistanceMetric distanceMetric = new ManhattanDistanceMetric( 1.0, 0.0, 10.0 );
			//ISpeciationStrategy<NeatGenome> speciationStrategy = new ParallelKMeansClusteringStrategy<NeatGenome>( distanceMetric, _parallelOptions );
			ISpeciationStrategy<NeatGenome> speciationStrategy = new KMeansClusteringStrategy<NeatGenome>( distanceMetric );

			// Create complexity regulation strategy.
			
			IComplexityRegulationStrategy complexityRegulationStrategy = new DefaultComplexityRegulationStrategy( ceilingType, complexityThreshold );

			// Create the evolution algorithm.
			Algorithm = new NeatEvolutionAlgorithm<NeatGenome>( eaParams, speciationStrategy, complexityRegulationStrategy );

			// Create IBlackBox evaluator.
			Evaluator = new UnicycleBlackBoxEvaluator(this);

			// Create genome decoder.
			IGenomeDecoder<NeatGenome, IBlackBox> genomeDecoder = new NeatGenomeDecoder( activationScheme );

			// Create a genome list evaluator. This packages up the genome decoder with the genome evaluator.
			//IGenomeListEvaluator<NeatGenome> innerEvaluator = new ParallelGenomeListEvaluator<NeatGenome, IBlackBox>( genomeDecoder, evaluator, _parallelOptions );
			IGenomeListEvaluator<NeatGenome> innerEvaluator = new UnicycleGenomeListEvaluator<NeatGenome, IBlackBox>( genomeDecoder, Evaluator );

			// Wrap the list evaluator in a 'selective' evaluator that will only evaluate new genomes. That is, we skip re-evaluating any genomes
			// that were in the population in previous generations (elite genomes). This is determined by examining each genome's evaluation info object.
			SelectiveEvaluator = new SelectiveGenomeListEvaluator<NeatGenome>(
																			innerEvaluator,
																			SelectiveGenomeListEvaluator<NeatGenome>.CreatePredicate_OnceOnly() );

			await Algorithm.Initialize( SelectiveEvaluator, GenomeFactory, GenomeList );
			//await Algorithm.Initialize( innerEvaluator, GenomeFactory, GenomeList );

		}

		private void Algorithm_UpdateEvent( object sender, EventArgs e )
		{
			Evaluator.Reset();
		}

		public override void StartGeneration()
		{
			Log.Info("continue generation");
			Algorithm.StartContinue();
		}

		public override async Task FinishGeneration()
		{
			Evaluator.FinishGeneration();
			Log.Info($"internal finish genome {Algorithm.CurrentGeneration}");
			// we don't have AutoResetEvent (facepunch moment)
			// so here's a crappy spin lock because can't have shit in detroit
			while ( Algorithm.RunState != RunState.Paused )
			{
				if ( Algorithm.RunState != RunState.Running )
				{
					Log.Info( $"Invalid run state {Algorithm.RunState}" );
					break;
				}
				//Log.Info( $"run state {Algorithm.RunState}" );
				await Task.Delay( 2 );
			}

			Evaluator.Reset();

			//Algorithm.RequestPauseAndWait();
		}
	}
}
