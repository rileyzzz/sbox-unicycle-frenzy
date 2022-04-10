using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharpNeat.Core
{
	/// <summary>
	/// A concrete implementation of IGenomeListEvaluator that evaluates genomes independently of each other
	/// and in series on a single thread. 
	/// 
	/// Genome decoding is performed by a provided IGenomeDecoder.
	/// Phenome evaluation is performed by a provided IPhenomeEvaluator.
	/// 
	/// This class evaluates on a single thread only, and therefore is a good choice when debugging code.
	/// </summary>
	/// <typeparam name="TGenome">The genome type that is decoded.</typeparam>
	/// <typeparam name="TPhenome">The phenome type that is decoded to and then evaluated.</typeparam>
	public class UnicycleGenomeListEvaluator<TGenome, TPhenome> : IGenomeListEvaluator<TGenome>
		where TGenome : class, IGenome<TGenome>
		where TPhenome : class
	{
		//readonly EvaluationMethod _evaluationMethod;
		readonly Func<IList<TGenome>, Task> _evaluationMethod;
		readonly IGenomeDecoder<TGenome, TPhenome> _genomeDecoder;
		readonly IPhenomeEvaluator<TPhenome> _phenomeEvaluator;
		readonly bool _enablePhenomeCaching;

		delegate void EvaluationMethod( IList<TGenome> genomeList );

		#region Constructor

		/// <summary>
		/// Construct with the provided IGenomeDecoder and IPhenomeEvaluator.
		/// Phenome caching is enabled by default.
		/// </summary>
		public UnicycleGenomeListEvaluator( IGenomeDecoder<TGenome, TPhenome> genomeDecoder,
										 IPhenomeEvaluator<TPhenome> phenomeEvaluator )
		{
			_genomeDecoder = genomeDecoder;
			_phenomeEvaluator = phenomeEvaluator;
			_enablePhenomeCaching = true;
			_evaluationMethod = Evaluate_Caching;
		}

		/// <summary>
		/// Construct with the provided IGenomeDecoder, IPhenomeEvaluator and enablePhenomeCaching flag.
		/// </summary>
		public UnicycleGenomeListEvaluator( IGenomeDecoder<TGenome, TPhenome> genomeDecoder,
										 IPhenomeEvaluator<TPhenome> phenomeEvaluator,
										 bool enablePhenomeCaching )
		{
			_genomeDecoder = genomeDecoder;
			_phenomeEvaluator = phenomeEvaluator;
			_enablePhenomeCaching = enablePhenomeCaching;

			if ( _enablePhenomeCaching )
			{
				_evaluationMethod = Evaluate_Caching;
			}
			else
			{
				_evaluationMethod = Evaluate_NonCaching;
			}
		}

		#endregion

		#region IGenomeListEvaluator<TGenome> Members

		/// <summary>
		/// Gets the total number of individual genome evaluations that have been performed by this evaluator.
		/// </summary>
		public ulong EvaluationCount
		{
			get { return _phenomeEvaluator.EvaluationCount; }
		}

		/// <summary>
		/// Gets a value indicating whether some goal fitness has been achieved and that
		/// the evolutionary algorithm/search should stop. This property's value can remain false
		/// to allow the algorithm to run indefinitely.
		/// </summary>
		public bool StopConditionSatisfied
		{
			get { return _phenomeEvaluator.StopConditionSatisfied; }
		}

		/// <summary>
		/// Evaluates a list of genomes. Here we decode each genome in series using the contained
		/// IGenomeDecoder and evaluate the resulting TPhenome using the contained IPhenomeEvaluator.
		/// </summary>
		public async Task Evaluate( IList<TGenome> genomeList )
		{
			await _evaluationMethod( genomeList );
		}

		/// <summary>
		/// Reset the internal state of the evaluation scheme if any exists.
		/// </summary>
		public void Reset()
		{
			_phenomeEvaluator.Reset();
		}

		#endregion

		#region Private Methods

		private async Task Evaluate_NonCaching( IList<TGenome> genomeList )
		{
			//var tasks = new List<Task>();
			_phenomeEvaluator.Reset();
			// Decode and evaluate each genome in turn.
			for ( int i = 0; i < genomeList.Count; i++ )
			{
				TGenome genome = genomeList[i];
				//var task = Sandbox.GameTask.RunInThreadAsync(async () => {
				TPhenome phenome = _genomeDecoder.Decode( genome );
				if ( null == phenome )
				{   // Non-viable genome.
					genome.EvaluationInfo.SetFitness( 0.0 );
					genome.EvaluationInfo.AuxFitnessArr = null;
				}
				else
				{
					FitnessInfo fitnessInfo = await _phenomeEvaluator.Evaluate( i, phenome );
					genome.EvaluationInfo.SetFitness( fitnessInfo._fitness );
					genome.EvaluationInfo.AuxFitnessArr = fitnessInfo._auxFitnessArr;
				}
				//} );
				//tasks.Add(task);
			}

			//foreach ( var task in tasks )
				//await task;
		}

		private async Task Evaluate_Caching( IList<TGenome> genomeList )
		{
			var tasks = new List<Task>();
			_phenomeEvaluator.Reset();

			//Log.Info($"evaluating {genomeList.Count} genomes");
			//TPhenome[] phenomes = new TPhenome[genomeList.Count];
			//for (int i = 0; i < genomeList.Count; i++ )
			//{
			//	TGenome genome = genomeList[i];
			//	TPhenome phenome = (TPhenome)genome.CachedPhenome;
			//	if ( null == phenome )
			//	{   // Decode the phenome and store a ref against the genome.
			//		phenome = _genomeDecoder.Decode( genome );
			//		genome.CachedPhenome = phenome;
			//	}

			//	phenomes[i] = phenome;
			//}

			//while (true)
			//{
			//	bool allComplete = true;

			//	for ( int i = 0; i < genomeList.Count; i++ )
			//	{
			//		TPhenome phenome = phenomes[i];
			//		_phenomeEvaluator.Run(i, phenome);
			//		allComplete &= _phenomeEvaluator.IsComplete(i);
			//	}

			//	if ( allComplete )
			//		break;

			//	await Task.Delay(10);
			//}

			//bool[] completion = new bool[genomeList.Count];
			// Decode and evaluate each genome in turn.
			//foreach ( TGenome genome in genomeList )
			for ( int i = 0; i < genomeList.Count; i++ )
			{
				// need a copy for lambda
				int index = i;

				//var task = Sandbox.GameTask.RunInThreadAsync( async () => {
				var task = Sandbox.GameTask.RunInThreadAsync( async () => {
					TGenome genome = genomeList[index];
					// Decode and evaluate each genome in turn.
					TPhenome phenome = (TPhenome)genome.CachedPhenome;
					if ( null == phenome )
					{   // Decode the phenome and store a ref against the genome.
						phenome = _genomeDecoder.Decode( genome );
						genome.CachedPhenome = phenome;
					}

					if ( null == phenome )
					{   // Non-viable genome.
						genome.EvaluationInfo.SetFitness( 0.0 );
						genome.EvaluationInfo.AuxFitnessArr = null;
					}
					else
					{
						FitnessInfo fitnessInfo = await _phenomeEvaluator.Evaluate( index, phenome );
						genome.EvaluationInfo.SetFitness( fitnessInfo._fitness );
						genome.EvaluationInfo.AuxFitnessArr = fitnessInfo._auxFitnessArr;
					}

					//completion[i] = true;
				} );
				tasks.Add( task );
			}

			Log.Info("tasks submitted");
			foreach ( var task in tasks )
				await task;
			Log.Info("tasks finished");
			//Log.Info("evaluate finished");
			//int test = 0;
			//while (true)
			//{
			//	bool allComplete = true;
			//	foreach ( var x in completion )
			//		allComplete &= x;

			//	if ( allComplete )
			//		break;

			//	Log.Info("evaluator suspended");
			//	await Task.Delay(1000);

			//	if (test++ > 10)
			//	{
			//		throw new Exception("test");
			//		//Log.Error("bruh");
			//	}
			//}
		}

		#endregion
	}
}
