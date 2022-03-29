using Sandbox;

internal class UnicycleAnimator : PawnAnimator
{

    private float balx = .5f;
    private float baly = .5f;
    private Angles prevtilt;

    public override void Simulate()
    {
        if ( Pawn is not UnicyclePlayer pl ) return;
        if ( !pl.Terry.IsValid() ) return;
        if ( !pl.Unicycle.IsValid() ) return;

        // todo: still hate how I'm targeting pl.Terry like this

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

        target.SetAnimParameter( "unicycle_balance_x", balx );
        target.SetAnimParameter( "unicycle_balance_y", baly );

        balx += Time.Delta * .25f * (pl.Tilt.pitch - prevtilt.pitch);
        baly += Time.Delta * .25f * (pl.Tilt.roll - prevtilt.roll);
        prevtilt = pl.Tilt;

        balx = balx.LerpTo( .5f, Time.Delta );
        baly = baly.LerpTo( .5f, Time.Delta );

        if ( pl.Controller is not UnicycleController ctrl ) return;

        var jumpcharge = InputActions.Jump.Down() ? (pl.TimeSinceJumpDown / ctrl.MaxJumpStrengthTime) : 0f;
        target.SetAnimParameter( "unicycle_jump_charge", jumpcharge );
    }

}
