using Sandbox;

internal class UnicycleAnimator : StandardPlayerAnimator
{
	public override void DoRotation( Rotation idealRotation )
	{
		if ( Pawn is not UnicyclePlayer pl ) return;

		// rotate when dev camera
		if ( pl.GetActiveController() is not UnicycleController )
		{
			Rotation = idealRotation;
			return;
		}

		if ( Pawn.IsServer ) return;

		var bone = pl.GetBoneIndex( "Wheel" );
		var tx = pl.GetBoneTransform( bone );
		var txRot = tx.WithRotation( tx.Rotation.RotateAroundAxis( Vector3.Left, Pawn.Velocity.WithZ(0).Length * Time.Delta ) );

		pl.SetBone( bone, txRot );
	}
}
