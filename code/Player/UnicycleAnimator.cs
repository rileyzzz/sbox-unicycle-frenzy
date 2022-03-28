using Sandbox;

internal class UnicycleAnimator : PawnAnimator
{

	public override void Simulate()
	{
		if ( Pawn is not UnicyclePlayer pl ) return;
		if ( !pl.Terry.IsValid() ) return;
		if ( !pl.Unicycle.IsValid() ) return;

		var target = pl.Terry;
		var unicycle = pl.Unicycle;

		var leftpos = unicycle.DisplayedLeftPedal.Position;
		var rightpos = unicycle.DisplayedRightPedal.Position;
		leftpos = target.Transform.PointToLocal( leftpos + Rotation.Left * 3 + Rotation.Up * 5 + Rotation.Forward );
		rightpos = target.Transform.PointToLocal( rightpos + Rotation.Right * 3 + Rotation.Up * 5 + Rotation.Forward );

		target.SetAnimParameter( "b_unicycle_enable_foot_ik", true );
		target.SetAnimParameter( "left_foot_ik.position", leftpos );
		target.SetAnimParameter( "right_foot_ik.position", rightpos );

		target.SetAnimParameter( "left_foot_ik.rotation", Rotation.From( 90, -90, 0 ) );
		target.SetAnimParameter( "right_foot_ik.rotation", Rotation.From( 90, -90, 0 ) );
	}

}
