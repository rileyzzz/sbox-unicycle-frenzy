using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Sandbox;

namespace UnicycleAI
{
	public abstract class NetworkModel
	{
		public List<NetworkAgent> Agents { get; protected set; }

		public NetworkModel()
		{
		}

		public virtual async Task Init(List<NetworkAgent> agents, int numInputs, int numOutputs )
		{
			Agents = agents;
		}

		public abstract void StartGeneration();
		public abstract Task FinishGeneration();
	}
}
