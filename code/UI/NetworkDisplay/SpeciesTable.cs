using Sandbox;
using Sandbox.UI;
using System.Linq;

[UseTemplate]
internal class SpeciesTable : Panel
{
	public Panel Canvas { get; set; }

	//Client[] Bots = new Client[0];
	int NumBots = 0;
	public override void Tick()
	{
		base.Tick();

		CheckSpeciesList();
		//Rebuild();
	}

	private void CheckSpeciesList()
	{

		var bots = Client.All.Where( x => x.IsBot ).ToArray();

		if ( bots.Length != NumBots )
		{
			NumBots = bots.Length;
			Rebuild();
		}
		else
		{
			Sort();
		}
	}

	protected override void PostTemplateApplied()
	{
		base.PostTemplateApplied();

		Rebuild();
	}

	public override void OnHotloaded()
	{
		base.OnHotloaded();

		Rebuild();
	}

	public void Rebuild()
	{
		//var sorted = Client.All.Where( x => x.IsBot && x.Pawn is UnicyclePlayer ).OrderByDescending( x => ((UnicyclePlayer)x.Pawn).BotFitness ).ToArray();
		var bots = Client.All.Where( x => x.IsBot && x.Pawn is UnicyclePlayer ).ToArray();
		Canvas.DeleteChildren( true );

		//for ( int i = 0; i < bots.Length; i++ )
		foreach (var bot in bots)
		{
			//Log.Info( "adding bot to species table" );
			var el = new SpeciesTableEntry( bot );
			el.Parent = Canvas;
		}

		Sort();
	}

	public void Sort()
	{
		Canvas.SortChildren( ( x, y ) => ((SpeciesTableEntry)y).Fitness.CompareTo( ((SpeciesTableEntry)x).Fitness ) );
	}
}
