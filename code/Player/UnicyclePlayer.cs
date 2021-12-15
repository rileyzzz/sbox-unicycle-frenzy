using Sandbox;
using System.Collections.Generic;

internal partial class UnicyclePlayer : Sandbox.Player
{

	[Net]
	public AnimEntity Terry { get; set; }
	[Net]
	public UnicycleEntity Unicycle { get; set; }
	[Net]
	public List<Checkpoint> Checkpoints { get; set; } = new();


	private Clothing.Container clothing;

	public override void Respawn()
	{
		base.Respawn();

		// todo: not sure I like this setup, might prefer it like CarEntity
		// so the player is actually a normal terry instead of an invisible entity w/ controller
		SetModel( "models/parts/seats/dev_seat.vmdl" );
		EnableDrawing = false;
		EnableAllCollisions = true;

		Unicycle = new UnicycleEntity();
		Unicycle.SetParent( this, null, Transform.Zero );

		Terry = new AnimEntity( "models/citizen/citizen.vmdl" );
		Terry.SetParent( Unicycle, null, Transform.Zero );
		Terry.SetAnimBool( "b_sit", true );

		Camera = new UnicycleCamera();
		Controller = new UnicycleController();
		Animator = new UnicycleAnimator();
		
		var c = Controller as UnicycleController;
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, c.Mins, c.Maxs );
		RemoveCollisionLayer( CollisionLayer.Solid );

		if ( clothing == null )
		{
			clothing = new();
			clothing.LoadFromClient( Client );
		}

		clothing.DressEntity( Terry );

		ResetMovement();
		GotoBestCheckpoint();
	}

	public override void OnKilled()
	{
		base.OnKilled();

		EnableAllCollisions = false;
		EnableDrawing = false;

		if ( Unicycle.IsValid() )
		{
			Unicycle.EnableDrawing = false;
			Unicycle.Delete();
		}

		if ( Terry.IsValid() )
		{
			Terry.EnableDrawing = false;
			Terry.DeleteAsync( 10f );
		}

		Camera = new SpectateRagdollCamera();

		RagdollOnClient();
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( GetActiveController() == DevController )
		{
			ResetMovement();
		}
	}

}

