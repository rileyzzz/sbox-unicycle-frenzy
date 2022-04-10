using System;
using Sandbox;

namespace UnicycleAI
{

	public interface UnicycleBrain
	{
		//public UnicycleBrain Clone();

		//public UnicycleBrain Cross( UnicycleBrain other );

		//public void Mutate( float mutationScale, float mutationChance );

		public void SetInputs( double[] inputs );

		public double[] GetOutputs();

		public void Execute();
	}

}
