using Sandbox;
using Sandbox.ScreenShake;

internal partial class UnicyclePlayer : Sandbox.Player
{

	[Net]
	public ModelEntity Unicycle { get; set; }

	private Clothing.Container clothing;

	public override void Respawn()
	{
		base.Respawn();

		SetModel( "models/citizen/citizen.vmdl" );

		Camera = new UnicycleCamera();
		Controller = new UnicycleController();
		Animator = new UnicycleAnimator();

		EnableDrawing = true;

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

		ShakeCameraOnClient();
		RagdollOnClient();
	}

	public void Fall()
	{
		Host.AssertServer();

		Game.Current.DoPlayerSuicide( Client );
	}

	[ClientRpc]
	private void ShakeCameraOnClient()
	{
		if ( IsLocalPawn )
		{
			new Perlin( 2f, 2, 3 );
		}
	}

	[ClientRpc]
	private void RagdollOnClient()
	{
		var ent = new ModelEntity();
		ent.Position = Position;
		ent.Rotation = Rotation;
		ent.Scale = Scale;
		ent.MoveType = MoveType.Physics;
		ent.UsePhysicsCollision = true;
		ent.EnableAllCollisions = true;
		ent.CollisionGroup = CollisionGroup.Debris;
		ent.SetModel( GetModelName() );
		ent.CopyBonesFrom( this );
		ent.CopyBodyGroups( this );
		ent.CopyMaterialGroup( this );
		ent.TakeDecalsFrom( this );
		ent.EnableHitboxes = true;
		ent.EnableAllCollisions = true;
		ent.SurroundingBoundsMode = SurroundingBoundsType.Physics;
		ent.RenderColor = RenderColor;
		ent.PhysicsGroup.Velocity = Velocity;

		if ( Local.Pawn == this )
		{
			//ent.EnableDrawing = false; wtf
		}

		ent.SetInteractsAs( CollisionLayer.Debris );
		ent.SetInteractsWith( CollisionLayer.WORLD_GEOMETRY );
		ent.SetInteractsExclude( CollisionLayer.Player | CollisionLayer.Debris );

		foreach ( var child in Children )
		{
			if ( !child.Tags.Has( "clothes" ) ) continue;
			if ( child is not ModelEntity e ) continue;

			var model = e.GetModelName();

			var clothing = new ModelEntity();
			clothing.SetModel( model );
			clothing.SetParent( ent, true );
			clothing.RenderColor = e.RenderColor;
			clothing.CopyBodyGroups( e );
			clothing.CopyMaterialGroup( e );
		}

		if ( this is Player pl )
		{
			pl.Corpse = ent;
		}

		ent.DeleteAsync( 10.0f );
	}

}

