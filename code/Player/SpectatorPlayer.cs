using Facepunch.Customization;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

internal partial class SpectatorPlayer : Sandbox.Player
{

	public override void Respawn()
	{
		base.Respawn();

		EnableDrawing = false;
		EnableAllCollisions = false;

		CameraMode = new FirstPersonCamera();
		Controller = new NoclipController();

		//var cp = Checkpoints.LastOrDefault();
		//if ( !cp.IsValid() )
		//{
		//	cp = Entity.All.FirstOrDefault( x => x is Checkpoint c && c.IsStart ) as Checkpoint;
		//	if ( cp == null ) return;
		//}
		var cp = Entity.All.FirstOrDefault( x => x is Checkpoint c && c.IsStart ) as Checkpoint;
		cp.GetSpawnPoint( out Vector3 position, out Rotation rotation );

		Position = position + Vector3.Up * 5;
		Rotation = rotation;
		Velocity = Vector3.Zero;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();
	}

	public override void OnKilled()
	{
		base.OnKilled();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );
	}

}

