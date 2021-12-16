using Sandbox;
using System;
using System.Collections.Generic;

internal partial class UnicyclePlayer : Sandbox.Player
{

	[Net]
	public AnimEntity Terry { get; set; }
	[Net]
	public UnicycleEntity Unicycle { get; set; }
	[Net]
	public List<Checkpoint> Checkpoints { get; set; } = new();

	public const float RespawnDelay = 3f;

	private TimeSince timeSinceDied;
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

		timeSinceDied = 0;

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
		if( LifeState == LifeState.Alive )
		{
			var controller = GetActiveController();
			controller?.Simulate( cl, this, GetActiveAnimator() );

			if ( GetActiveController() == DevController )
			{
				ResetMovement();
				ResetTimer();
			}
		}

		if( LifeState == LifeState.Dead )
		{
			if( IsServer && timeSinceDied > RespawnDelay )
				Respawn();
		}

		if ( Input.Pressed( InputButton.Drop ) )
		{
			Fall();
			ResetTimer();
			timeSinceDied = RespawnDelay - .5f;
		}

		if ( Input.Pressed( InputButton.Reload ) )
		{
			Fall();
			timeSinceDied = RespawnDelay - .5f;
		}
	}

}

