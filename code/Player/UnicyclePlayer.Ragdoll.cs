using Sandbox;
using Sandbox.ScreenShake;

internal partial class UnicyclePlayer
{

	private void RagdollModel( ModelEntity modelEnt, bool isCorpse )
	{
		if ( !modelEnt.IsValid() )
		{
			Log.Error( "??" );
			return;
		}

		var ent = new ModelEntity();
		ent.Position = Position;
		ent.Rotation = Rotation;
		ent.Scale = Scale;
		ent.MoveType = MoveType.Physics;
		ent.UsePhysicsCollision = true;
		ent.EnableAllCollisions = true;
		ent.CollisionGroup = CollisionGroup.Debris;
		ent.SetModel( modelEnt.GetModelName() );
		ent.CopyBonesFrom( modelEnt );
		ent.CopyBodyGroups( modelEnt );
		ent.CopyMaterialGroup( modelEnt );
		ent.TakeDecalsFrom( modelEnt );
		ent.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
		ent.EnableHitboxes = true;
		ent.EnableAllCollisions = true;
		ent.SurroundingBoundsMode = SurroundingBoundsType.Physics;
		ent.RenderColor = RenderColor;
		ent.PhysicsGroup.Velocity = Velocity;

		if ( Local.Pawn == this )
		{
			new Perlin( 2f, 2, 3 );
			//ent.EnableDrawing = false; wtf
		}

		ent.SetInteractsAs( CollisionLayer.Debris );
		ent.SetInteractsWith( CollisionLayer.WORLD_GEOMETRY );
		ent.SetInteractsExclude( CollisionLayer.Player | CollisionLayer.Debris );

		foreach ( var child in modelEnt.Children )
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

		if ( isCorpse && this is Player pl )
		{
			pl.Corpse = ent;
		}

		ent.DeleteAsync( 10.0f );
	}

	[ClientRpc]
	private void RagdollOnClient()
	{
		// todo: might be able to tidy up the player's hierarchy and networked life/death cycle 
		//		 so we're not paranoid about an nre here
		//		 maybe also predicted ragdolling to smooth out high ping deaths

		if ( Terry.IsValid() )
		{
			RagdollModel( Terry, true );
		}

		if ( Unicycle.IsValid() )
		{
			RagdollModel( Unicycle.FrameModel, false );
			RagdollModel( Unicycle.WheelModel, false );
			RagdollModel( Unicycle.SeatModel, false );
		}
	}

}

