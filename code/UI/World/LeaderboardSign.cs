using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;

[UseTemplate]
internal class LeaderboardSign : WorldPanel
{

	public static List<LeaderboardSign> All = new();

	private LeaderboardPost post;
	private int hash;

	public LeaderboardSign( LeaderboardPost post )
	{
		this.post = post;

		All.Add( this );

		Reposition();
		Rebuild();
	}

	public override void Tick()
	{
		base.Tick();

		if ( !post.IsValid() )
		{
			Delete();
			return;
		}

		var newhash = LdbHash();
		if ( hash == newhash ) return;
		hash = newhash;

		Rebuild();
	}

	public override void OnDeleted()
	{
		base.OnDeleted();

		All.Remove( this );
	}

	public void Rebuild()
	{
		DeleteChildren();

		var players = Player.All.Where( x => x is UnicyclePlayer && x.IsValid() && x.Client.IsValid() ).ToList();
		players.OrderBy( x => (x as UnicyclePlayer).BestTime );

		int rank = 1;

		foreach( var player in players )
		{
			if ( player is not UnicyclePlayer pl ) continue;
			var entry = new LeaderboardSignEntry( rank, pl );
			entry.Parent = this;
			rank++;
		}
	}

	private int LdbHash()
	{
		var result = 0;
		foreach( var player in Player.All )
		{
			if ( player is not UnicyclePlayer pl || !pl.Client.IsValid() ) continue;
			result = HashCode.Combine( result, pl.BestTime );
		}
		return result;
	}

	private void Reposition()
	{
		var width = 2300;
		var height = 1450;
		MaxInteractionDistance = 5000f;
		PanelBounds = new Rect( -width * .5f, -height * .5f, width, height );
		Position = post.Position + post.Rotation.Forward * .01f + Vector3.Up * 82;
		Rotation = post.Rotation;
	}

}

