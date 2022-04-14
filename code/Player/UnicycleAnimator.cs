
using Sandbox;

internal class UnicycleAnimator : PawnAnimator
{

	public override void Simulate()
    {
        if ( Pawn is not UnicyclePlayer pl ) return;
        if ( !pl.Terry.IsValid() ) return;
        if ( !pl.Unicycle.IsValid() ) return;
		if ( !BotManager.DrawPlayers ) return;

        var target = pl.Terry;
        var unicycle = pl.Unicycle;

		var speed = Pawn.Velocity.WithZ( 0 ).Length;
		target.SetAnimParameter( "move_groundspeed", speed );

		var aimPos = Pawn.EyePosition + Input.Rotation.Forward * 200 + Vector3.Up * 50;
		target.SetAnimLookAt( "aim_eyes", aimPos );
		target.SetAnimLookAt( "aim_head", aimPos );

		var leftpos = unicycle.DisplayedLeftPedal?.GetAttachment( "foot" )?.Position ?? Vector3.Zero;
        var rightpos = unicycle.DisplayedRightPedal?.GetAttachment( "foot" )?.Position ?? Vector3.Zero;
        leftpos = target.Transform.PointToLocal( leftpos + Rotation.Up * 5 );
        rightpos = target.Transform.PointToLocal( rightpos + Rotation.Up * 5 );

        target.SetAnimParameter( "b_unicycle_enable_foot_ik", true );
        target.SetAnimParameter( "left_foot_ik.position", leftpos );
        target.SetAnimParameter( "right_foot_ik.position", rightpos );

        target.SetAnimParameter( "left_foot_ik.rotation", Rotation.From( 90, -90, 0 ) );
        target.SetAnimParameter( "right_foot_ik.rotation", Rotation.From( 90, -90, 0 ) );

		var a = pl.PedalPosition.LerpInverse( -1f, 1f ) * .5f;
		target.SetAnimParameter( "unicycle_pedaling", a );

        if ( pl.Controller is not UnicycleController ctrl ) return;

        var jumpcharge = InputActions.Jump.Down() ? (pl.TimeSinceJumpDown / ctrl.MaxJumpStrengthTime) : 0f;
        target.SetAnimParameter( "unicycle_jump_charge", jumpcharge );

		var targetbalx = .5f + ( pl.Tilt.pitch / ctrl.MaxLean * .5f );
		var targetbaly = .5f + ( pl.Tilt.roll / ctrl.MaxLean * .5f );

		var balx = target.GetAnimParameterFloat( "unicycle_balance_x" );
		var baly = target.GetAnimParameterFloat( "unicycle_balance_y" );

		balx = balx.LerpTo( targetbalx, Time.Delta * 3f );
		baly = baly.LerpTo( targetbaly, Time.Delta * 3f );

		target.SetAnimParameter( "unicycle_balance_x", balx );
		target.SetAnimParameter( "unicycle_balance_y", baly );

		var targetLeanX = Input.Forward.LerpInverse( -1f, 1f );
		var targetLeanY = 1f - Input.Left.LerpInverse( -1f, 1f );

		var leanx = target.GetAnimParameterFloat( "unicycle_lean_x" );
		var leany = target.GetAnimParameterFloat( "unicycle_lean_y" );

		leanx = leanx.LerpTo( targetLeanX, Time.Delta * 7f );
		leany = leany.LerpTo( targetLeanY, Time.Delta * 7f );

		target.SetAnimParameter( "unicycle_lean_x", leanx );
		target.SetAnimParameter( "unicycle_lean_y", leany );
	}

}
