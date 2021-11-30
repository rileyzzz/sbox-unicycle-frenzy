using Sandbox;

internal class UnicycleAnimator : StandardPlayerAnimator
{
	public override void DoRotation( Rotation idealRotation )
	{
		if ( Pawn is not UnicyclePlayer pl || pl.Health <= 0 ) return;
		if ( pl.GetActiveController() is UnicycleController ) return;

		// rotate when dev camera
		base.DoRotation( idealRotation );
	}
}
