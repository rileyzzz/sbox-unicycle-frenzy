using Sandbox.UI;
using System.Linq;

[UseTemplate]
[NavigatorTarget("menu/leaderboards")]
internal class LeaderboardsTab : Panel
{

	public Panel Canvas { get; set; }

	protected override void PostTemplateApplied()
	{
		base.PostTemplateApplied();

		BuildEntries();
	}

	public override void OnHotloaded()
	{
		base.OnHotloaded();

		BuildEntries();
	}

	private void BuildEntries()
	{
		//Canvas.DeleteChildren();

		//var time = 5.32f;
		//for ( int i = 1; i < 102; i++ )
		//{
		//	var entry = new LeaderboardsTabEntry( i, RandomString( 24 ), time );

		//	if ( i == 10 ) entry.AddClass( "me" );

		//	Canvas.AddChild( entry );
		//	time += random.NextSingle() * 20;
		//}
	}

	private static System.Random random = new();

	public static string RandomString( int length )
	{
		const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		return new string( Enumerable.Repeat( chars, length )
			.Select( s => s[random.Next( s.Length )] ).ToArray() );
	}

}
