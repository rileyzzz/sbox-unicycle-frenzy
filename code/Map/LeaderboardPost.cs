using Hammer;
using Sandbox;


[Hammer.Model( Model = "models/leaderboard_post.vmdl")]
[Library( "uf_leaderboard", Description = "Shows a leaderboard in-game" )]
[EntityTool( "Leaderboard Post", "Unicycle Frenzy", "Shows a leaderboard in-game" )]
internal class LeaderboardPost : ModelEntity
{
	public override void Spawn()
	{
		base.Spawn();

		//SetModel( "models/leaderboard_post.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Static );

		EnableAllCollisions = true;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		new LeaderboardSign( this );
	}
}

