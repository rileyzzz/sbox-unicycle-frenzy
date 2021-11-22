using Sandbox;

internal class UnicycleAnimator : StandardPlayerAnimator
{
	public override void DoRotation( Rotation idealRotation )
	{
		if ( Pawn is not Player pl ) return;
		if ( pl.GetActiveController() is UnicycleController ) return;

		Rotation = idealRotation;
	}
}
