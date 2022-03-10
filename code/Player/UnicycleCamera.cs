using Sandbox;
using System;
using System.Collections.Generic;
using System.Numerics;

internal class UnicycleCamera : CameraMode
{

	private List<UfProp> viewblockers = new();
	private Vector2 controllerrot;

	public override void Update()
	{
		var pawn = Local.Pawn as UnicyclePlayer;

		if ( pawn == null ) return;
		if ( pawn.SpectateTarget.IsValid() ) pawn = pawn.SpectateTarget;

		ClearViewBlockers();
		UpdateViewBlockers( pawn );

		var targetRot = Input.Rotation;

		if ( Input.UsingController )
		{
			var angles = Input.Rotation.Angles();
			var delta = Input.GetAnalog( InputAnalog.Look );
			float mx = delta.x * 8f;
			float my = delta.y * 8f;
			var targetAng = angles + new Vector3( -my, -mx, 0f );
			targetAng.pitch = Math.Clamp( targetAng.pitch, - 65f, 65f );

			targetRot = Rotation.From( targetAng );
		}

		var center = pawn.Position + Vector3.Up * 80;
		var distance = 150.0f * pawn.Scale;
		var targetPos = center + targetRot.Forward * -distance;

		var tr = Trace.Ray( center, targetPos )
			.Ignore( pawn )
			.Radius( 8 )
			.Run();

		var endpos = tr.EndPosition;

		if ( tr.Entity is UfProp ufp )
		{
			if ( ufp.NoCameraCollide )
				endpos = targetPos;
		}

		Position = endpos;
		Rotation = targetRot;

		var rot = pawn.Rotation.Angles() * .015f;
		rot.yaw = 0;

		Rotation *= Rotation.From( rot );

		if ( Input.UsingController && pawn.Velocity.WithZ( 0 ).Length > 50 )
		{
			var snapback = pawn.TargetForward.Angles();
			snapback.roll = 0;
			snapback.pitch = 30;
			Rotation = Rotation.Lerp( Rotation, Rotation.From( snapback ), Time.Delta );
		}

		var spd = pawn.Velocity.WithZ( 0 ).Length / 350f;
		var fov = 82f.LerpTo( 92f, spd );

		FieldOfView = FieldOfView.LerpTo( fov, Time.Delta );

		Viewer = null;
	}

	public override void BuildInput( InputBuilder input )
	{
		base.BuildInput( input );

		if ( !Input.UsingController ) return;

		// controllers get special handling so update ViewAngles here
		input.ViewAngles = Rotation.Angles();
	}

	public override void Activated()
	{
		base.Activated();

		FieldOfView = 85;
	}

	public override void Deactivated()
	{
		base.Deactivated();

		ClearViewBlockers();
	}

	private void ClearViewBlockers()
	{
		foreach ( var ent in viewblockers )
		{
			ent.BlockingView = false;
		}
		viewblockers.Clear();
	}

	private void UpdateViewBlockers( UnicyclePlayer pawn )
	{
		var traces = Trace.Sphere( 3f, CurrentView.Position, pawn.Position + Vector3.Up * 16 ).RunAll();

		if ( traces == null ) return;

		foreach(var tr in traces )
		{
			if ( tr.Entity is not UfProp prop ) continue;
			prop.BlockingView = true;
			viewblockers.Add( prop );
		}
	}

}
