using Sandbox;

internal class UnicycleAnimator : PawnAnimator
{

	public override void Simulate()
    {
        if ( Pawn is not UnicyclePlayer pl ) return;
        if ( !pl.Terry.IsValid() ) return;
        if ( !pl.Unicycle.IsValid() ) return;

        // todo: still hate how I'm targeting pl.Terry like this

        var target = pl.Terry;
        var unicycle = pl.Unicycle;

        var leftpos = unicycle.DisplayedLeftPedal.GetAttachment( "foot" ).Value.Position;
        var rightpos = unicycle.DisplayedRightPedal.GetAttachment( "foot" ).Value.Position;
        leftpos = target.Transform.PointToLocal( leftpos + Rotation.Up * 5 );
        rightpos = target.Transform.PointToLocal( rightpos + Rotation.Up * 5 );

        target.SetAnimParameter( "b_unicycle_enable_foot_ik", true );
        target.SetAnimParameter( "left_foot_ik.position", leftpos );
        target.SetAnimParameter( "right_foot_ik.position", rightpos );

        target.SetAnimParameter( "left_foot_ik.rotation", Rotation.From( 90, -90, 0 ) );
        target.SetAnimParameter( "right_foot_ik.rotation", Rotation.From( 90, -90, 0 ) );

        if ( pl.Controller is not UnicycleController ctrl ) return;

        var jumpcharge = InputActions.Jump.Down() ? (pl.TimeSinceJumpDown / ctrl.MaxJumpStrengthTime) : 0f;
        target.SetAnimParameter( "unicycle_jump_charge", jumpcharge );

		var targetbalx = .5f + ( pl.Tilt.pitch / ctrl.MaxLean * .5f );
		var targetbaly = .5f + ( pl.Tilt.roll / ctrl.MaxLean * .5f );

		var balx = target.GetAnimParameterFloat( "unicycle_balance_x" );
		var baly = target.GetAnimParameterFloat( "unicycle_balance_y" );

		balx = balx.LerpTo( targetbalx, Time.Delta * 6f );
		baly = baly.LerpTo( targetbaly, Time.Delta * 6f );

		target.SetAnimParameter( "unicycle_balance_x", balx );
		target.SetAnimParameter( "unicycle_balance_y", baly );
	}

}
