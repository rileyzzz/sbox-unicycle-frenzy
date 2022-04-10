using Sandbox;
using Sandbox.UI;
using System.Linq;

[UseTemplate]
internal class SpeciesTableEntry : Panel
{
	public Client Bot;

	//public int Rank { get; set; }
	public int Rank => SiblingIndex;
	public string Name => Bot.Name ?? "Unknown";

	public float Fitness
	{
		get
		{
			if ( Bot.Pawn is not UnicyclePlayer player )
				return 0;

			return player.BotFitness;
		}
	}

	public string FitnessString => Fitness.ToString("0.00");

	public SpeciesTableEntry( Client bot )
	{
		//Rank = rank;
		Bot = bot;
	}

	public override void Tick()
	{
		base.Tick();

		if ( Bot.Pawn is not UnicyclePlayer player )
			return;

		SetClass("active", player.LifeState == LifeState.Alive);
	}
}
