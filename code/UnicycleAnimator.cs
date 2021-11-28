using Sandbox;
using System;

internal class UnicycleAnimator : StandardPlayerAnimator
{

	private float prevPedalPosition;

	public override void FrameSimulate()
	{
		base.FrameSimulate();

		if ( Pawn is not UnicyclePlayer pl || pl.Health <= 0 ) return;

		// wheel
		var tx = pl.GetBoneTransform( "Wheel" );
		var radius = tx.Position.z - Position.z;
		var distance = Velocity.Length;
		var angle = distance * (180f / (float)Math.PI) / radius;
		var txRot = tx.WithRotation( tx.Rotation.RotateAroundAxis( Vector3.Left, angle * Time.Delta ) );
		pl.SetBone( "Wheel", txRot );

		// pedals
		// todo: lerp towards target rotation
		var tx2 = pl.GetBoneTransform( "Pedals" );
		var pedalDelta = pl.PedalPosition - prevPedalPosition;
		if ( pedalDelta.AlmostEqual( 0f ) ) return;
		var pedalAngle = MathF.Abs( pedalDelta ) * 90f;
		var pedalRot = tx2.Rotation.RotateAroundAxis( Vector3.Left, pedalAngle );

		var tx2Rot = tx2.WithRotation( pedalRot );
		pl.SetBone( "Pedals", tx2Rot );

		prevPedalPosition = pl.PedalPosition;
	}

	public override void DoRotation( Rotation idealRotation )
	{
		if ( Pawn is not UnicyclePlayer pl || pl.Health <= 0 ) return;
		if ( pl.GetActiveController() is UnicycleController ) return;

		// rotate when dev camera
		base.DoRotation( idealRotation );
	}

}
