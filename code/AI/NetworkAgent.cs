using System;
using Sandbox;

namespace UnicycleAI
{
	public interface NetworkAgent
	{
		public double[] GetInputs();
		public void SetOutputs(double[] outputs);

		public float GetFitness();

		public bool IsActive();
	}
}
