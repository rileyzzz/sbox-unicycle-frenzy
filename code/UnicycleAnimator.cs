using Sandbox;

internal class UnicycleAnimator : StandardPlayerAnimator
{

	private float prevPedal;

	public void ResetPedals()
	{
		prevPedal = 0f;

		if ( Pawn is not UnicyclePlayer pl ) return;

		pl.ResetBone( "Pedals" );
	}

	public override void DoRotation( Rotation idealRotation )
	{
		if ( Pawn is not UnicyclePlayer pl || pl.Health <= 0 ) return;
		
		// rotate when dev camera
		if ( pl.GetActiveController() is not UnicycleController )
		{
			Rotation = idealRotation;
			ResetPedals();
			return;
		}

		if ( Pawn.IsServer ) return;

		// todo: bet animgraph can do this better

		var wheel = pl.GetBoneIndex( "Wheel" );
		var tx = pl.GetBoneTransform( wheel );
		var txRot = tx.WithRotation( tx.Rotation.RotateAroundAxis( Vector3.Left, Pawn.Velocity.WithZ( 0 ).Length * Time.Delta ) );

		pl.SetBone( wheel, txRot );

		var pedalDelta = System.Math.Abs( pl.PedalPosition - prevPedal );

		var pedals = pl.GetBoneIndex( "Pedals" );
		var tx2 = pl.GetBoneTransform( pedals );
		var tx2Rot = tx2.WithRotation( tx2.Rotation.RotateAroundAxis( Vector3.Left, pedalDelta * 100f ) );

		pl.SetBone( pedals, tx2Rot );

		prevPedal = pl.PedalPosition;
	}

}
