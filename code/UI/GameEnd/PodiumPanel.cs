
using Sandbox;
using Sandbox.UI;
using System.Linq;

[UseTemplate]
internal class PodiumPanel : Panel
{

	public int Rank { get; }
	public string Name { get; private set; }
	public string Time { get; private set; }

	public PodiumPanel( int rank )
	{
		Rank = rank;

		AddClass( rank.ToString() );
		UpdateData();
	}

	public override void Tick()
	{
		base.Tick();

		UpdateData();
	}

	private void UpdateData()
	{
		var players = Player.All.Where( x => x is UnicyclePlayer && x.IsValid() && x.Client.IsValid() ).ToList();
		var orderedPlayers = players.OrderBy( x => (x as UnicyclePlayer).BestTime );

		var player = orderedPlayers.Skip( Rank - 1 ).FirstOrDefault() as UnicyclePlayer;
		if ( player == null ) return;

		Name = player.Client.Name;
		Time = CourseTimer.FormattedTimeMsf( player.BestTime );
	}

}
