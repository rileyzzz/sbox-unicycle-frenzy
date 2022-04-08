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
	public string Fitness
	{
		get
		{
			if ( Bot.Pawn is not UnicyclePlayer player )
				return "";

			return player.BotFitness.ToString();
		}
	}

	public SpeciesTableEntry( Client bot )
	{
		//Rank = rank;
		Bot = bot;
	}
}
