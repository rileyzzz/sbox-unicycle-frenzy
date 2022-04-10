using System;
using System.Diagnostics;
using System.Threading.Tasks;
using SharpNeat.Core;
using SharpNeat.Phenomes;

namespace UnicycleAI
{
	public class UnicycleBlackBoxEvaluator : IPhenomeEvaluator<IBlackBox>
	{
		NEATModel Model;
		//object evalLock = new();
		ulong evalCount = 0;
		//bool stopConditionSatisfied;
		// always do one generation at a time
		bool stopConditionSatisfied = true;
		//bool generationFinished = false;

		public UnicycleBlackBoxEvaluator(NEATModel model)
		{
			Model = model;
		}

		public ulong EvaluationCount
		{
			get { return evalCount; }
		}

		public bool StopConditionSatisfied
		{
			get { return stopConditionSatisfied; }
		}

		public void Reset()
		{
			//lock ( evalLock )
			//{
			//	evalCount = 0;
			//}
			//stopConditionSatisfied = false;
		}

		public void FinishGeneration()
		{
			//stopConditionSatisfied = true;
		}
		//public bool IsComplete( int index )
		//{
		//	NetworkAgent agent = Model.Agents[index];
		//	return !agent.IsActive();
		//}

		//public void Run( int index, IBlackBox box )
		//{
		//	ISignalArray inputArr = box.InputSignalArray;
		//	ISignalArray outputArr = box.OutputSignalArray;

		//	NetworkAgent agent = Model.Agents[index];

		//	double[] inputs = agent.GetInputs();
		//	inputArr.CopyFrom( inputs, 0 );
		//	box.Activate();

		//	double[] outputs = new double[outputArr.Length];
		//	outputArr.CopyTo( outputs, 0 );
		//	agent.SetOutputs( outputs );
		//}

		public async Task<FitnessInfo> Evaluate( int index, IBlackBox box )
		{
			evalCount++;

			ISignalArray inputArr = box.InputSignalArray;
			ISignalArray outputArr = box.OutputSignalArray;

			//Log.Info($"evaluating agent {index}/{Model.Agents.Count}");
			NetworkAgent agent = Model.Agents[index];

			//NetworkAgent agent = Model.Agents[agentIndex];

			while ( agent.IsActive() )
			{
				box.ResetState();
				inputArr.Reset();

				double[] inputs = agent.GetInputs();
				inputArr.CopyFrom( inputs, 0 );

				box.Activate();

				double[] outputs = new double[outputArr.Length];
				outputArr.CopyTo( outputs, 0 );
				agent.SetOutputs( outputs );

				//Log.Info( $"inputs {string.Join( " ", inputs )}, outputs {string.Join( " ", outputs )}" );
				await Task.Delay( 2 );
			}

			//if ( !agent.IsActive() )
			//stopConditionSatisfied = true;

			double fitness = agent.GetFitness();
			if ( fitness < 0.0 || double.IsNaN(fitness) || double.IsInfinity(fitness) )
				fitness = 0.0;

			return new FitnessInfo( fitness, fitness );
		}
	}
}
