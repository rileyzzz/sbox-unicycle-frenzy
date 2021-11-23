using Sandbox;
using System.Collections.Generic;
using System.Linq;

internal partial class UnicyclePlayer : Sandbox.Player
{

	[Net]
	public AnimEntity Terry { get; set; }
	[Net]
	public List<Checkpoint> Checkpoints { get; set; } = new();


	private Clothing.Container clothing;

	public override void Respawn()
	{
		base.Respawn();

		SetModel( "models/unicycle_dev.vmdl" );
		ResetMovement();

		Camera = new UnicycleCamera();
		Controller = new UnicycleController();
		Animator = new UnicycleAnimator();

		EnableAllCollisions = true;
		EnableDrawing = true;

		Terry = new AnimEntity( "models/citizen/citizen.vmdl" );
		Terry.SetParent( this, "Seat", new Transform( Vector3.Zero, Rotation.Identity, 1f ) );
		Terry.LocalPosition = Vector3.Down * 30;

		var c = Controller as UnicycleController;
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, c.Mins, c.Maxs );

		if ( clothing == null )
		{
			clothing = new();
			clothing.LoadFromClient( Client );
		}

		clothing.DressEntity( Terry );

		GotoLastCheckpoint();
	}

	public override void OnKilled()
	{
		base.OnKilled();

		EnableAllCollisions = false;
		EnableDrawing = false;

		Terry.EnableDrawing = false;
		Terry.DeleteAsync( 10 );

		Camera = new SpectateRagdollCamera();

		RagdollOnClient();
	}

	public void ClearCheckpoints()
	{
		Host.AssertServer();

		Checkpoints.Clear();
	}

	public void TrySetCheckpoint( Checkpoint checkpoint )
	{
		Host.AssertServer();

		if ( Checkpoints.Contains( checkpoint ) ) return;
		Checkpoints.Add( checkpoint );
	}

	public void GotoLastCheckpoint()
	{
		Host.AssertServer();

		var cp = Checkpoints.LastOrDefault();
		if ( !cp.IsValid() ) return;

		ResetInterpolation();
		ResetMovement();

		cp.GetSpawnPoint( out Vector3 position, out Rotation rotation );
		Position = position + Vector3.Up * 5;
		Rotation = rotation;
		Velocity = Vector3.Zero;

		SetRotationOnClient( rotation );
	}

}

