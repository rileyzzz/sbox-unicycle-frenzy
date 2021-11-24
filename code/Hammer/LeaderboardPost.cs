using Hammer;
using Sandbox;

[EditorModel("models/leaderboard_post.vmdl")]
[Library( "uf_leaderboard", Description = "Shows a leaderboard in-game" )]
[EntityTool( "Leaderboard Post", "Unicycle Frenzy", "Shows a leaderboard in-game" )]
internal class LeaderboardPost : ModelEntity
{
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/leaderboard_post.vmdl" );
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		new LeaderboardSign( this );
	}
}

