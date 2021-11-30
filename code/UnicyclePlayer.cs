using Sandbox;
using System.Collections.Generic;
using System.Linq;

internal partial class UnicyclePlayer : Sandbox.Player
{

	[Net]
	public UnicycleEntity Unicycle { get; set; }
	[Net]
	public List<Checkpoint> Checkpoints { get; set; } = new();


	private Clothing.Container clothing;

	public override void Respawn()
	{
		base.Respawn();

		SetModel( "models/citizen/citizen.vmdl" );

		Unicycle = new UnicycleEntity();
		Unicycle.SetParent( this, null, Transform.Zero.WithScale( 1 ) );

		Camera = new UnicycleCamera();
		Controller = new UnicycleController();
		Animator = new UnicycleAnimator();

		EnableAllCollisions = true;
		EnableDrawing = true;

		var c = Controller as UnicycleController;
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, c.Mins, c.Maxs );

		if ( clothing == null )
		{
			clothing = new();
			clothing.LoadFromClient( Client );
		}

		clothing.DressEntity( this );

		ResetMovement();
		GotoLastCheckpoint();
	}

	public override void OnKilled()
	{
		base.OnKilled();

		EnableAllCollisions = false;
		EnableDrawing = false;

		if ( Unicycle.IsValid() )
		{
			Unicycle.EnableDrawing = false;
			Unicycle.DeleteAsync( 10f );
		}

		Camera = new SpectateRagdollCamera();

		RagdollOnClient();
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( GetActiveController() == DevController )
		{
			TargetForward = Rotation;
			Tilt = Angles.Zero;
		}
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

		cp.GetSpawnPoint( out Vector3 position, out Rotation rotation );
		Position = position + Vector3.Up * 5;
		Rotation = rotation;
		Velocity = Vector3.Zero;

		SetRotationOnClient( rotation );
		ResetInterpolation();
		ResetMovement();
	}

}

