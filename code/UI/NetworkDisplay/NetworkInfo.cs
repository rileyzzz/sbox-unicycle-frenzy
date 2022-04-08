using Sandbox;
using Sandbox.UI;
using System;
using System.Linq;

[UseTemplate]
internal class NetworkInfo : Panel
{
	public string Generation => "Generation " + BotManager.Generation.ToString();

	public Client Leader
	{
		get
		{
			var bots = Client.All.Where( x => x.IsBot && x.Pawn is UnicyclePlayer )
				//.Select( x => x.Pawn as UnicyclePlayer )
				.OrderByDescending( x => ((UnicyclePlayer)x.Pawn).BotFitness );
			return bots.FirstOrDefault();
		}
	}

	public string LeaderName => Leader.Name ?? "Unknown";
	
	public override void Tick()
	{
		base.Tick();

	}
}
