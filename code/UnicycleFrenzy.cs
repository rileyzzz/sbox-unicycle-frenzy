using Sandbox;

partial class UnicycleFrenzy : Sandbox.Game
{

	public UnicycleFrenzy()
	{

		if ( IsClient )
		{
			new UnicycleHud();
		}

	}

	public override void ClientJoined( Client cl )
	{
		base.ClientJoined( cl );

		cl.Pawn = new UnicyclePlayer();
		(cl.Pawn as Player).Respawn();
	}

}
