using Sandbox;
using System.Collections.Generic;
using System.Linq;

internal partial class UnicyclePlayer : Sandbox.Player
{

	[Net]
	public ModelEntity Unicycle { get; set; }
	[Net]
	public List<Checkpoint> Checkpoints { get; set; } = new();


	private Clothing.Container clothing;

	public override void Respawn()
	{
		base.Respawn();

		SetModel( "models/citizen/citizen.vmdl" );
		ResetMovement();

		Camera = new UnicycleCamera();
		Controller = new UnicycleController();
		Animator = new UnicycleAnimator();

		EnableAllCollisions = true;
		EnableDrawing = true;

		var c = Controller as UnicycleController;
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, c.Mins, c.Maxs );

		Unicycle = new ModelEntity();
		Unicycle.SetModel( "models/citizen_props/wheel01.vmdl" );
		Unicycle.SetParent( this, null, new Transform( 0f, Rotation.Identity, .5f ) );
		Unicycle.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
		

		if ( clothing == null )
		{
			var hat = new Clothing()
			{
				Model = "models/citizen_clothes/hat/hat_hardhat.vmdl",
				Category = Clothing.ClothingCategory.Hat
			};
			clothing = new();
			clothing.Clothing.Add( hat );
		}

		clothing.DressEntity( this );

		GotoLastCheckpoint();
	}

	public override void OnKilled()
	{
		base.OnKilled();

		Unicycle.SetParent( null );
		Unicycle.Velocity = Velocity;
		Unicycle.DeleteAsync( 10 );
		Unicycle = null;

		EnableAllCollisions = false;
		EnableDrawing = false;

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

