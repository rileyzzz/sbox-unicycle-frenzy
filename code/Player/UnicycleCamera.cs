﻿
using Sandbox;
using System;
using System.Collections.Generic;

internal class UnicycleCamera : CameraMode
{

	private List<UfProp> viewblockers = new();

	public override void Update()
	{
		var pawn = Local.Pawn as UnicyclePlayer;

		if ( pawn == null ) return;
		if ( pawn.SpectateTarget.IsValid() ) pawn = pawn.SpectateTarget;

		ClearViewBlockers();
		UpdateViewBlockers( pawn );

		var viewangles = Input.Rotation.Angles();

		// hack in sensitivity boost until we get sens slider for controllers
		if ( Input.UsingController )
		{
			var delta = Input.GetAnalog( InputAnalog.Look );
			viewangles += new Vector3( -delta.y * 6f, -delta.x * 6f, 0f );
		}

		viewangles.pitch = Math.Clamp( viewangles.pitch, -35f, 65f );

		var targetRot = Rotation.From( viewangles );
		var center = pawn.Position + Vector3.Up * 80;
		var distance = 150.0f * pawn.Scale;
		var targetPos = center + targetRot.Forward * -distance;

		var tr = Trace.Ray( center, targetPos )
			.Ignore( pawn )
			.Radius( 8 )
			.Run();

		var endpos = tr.EndPosition;

		if ( tr.Entity is UfProp ufp && ufp.NoCameraCollide )
			endpos = targetPos;

		Position = endpos;
		Rotation = targetRot;

		// controller constantly tries to center itself
		if ( Input.UsingController && pawn.Velocity.WithZ( 0 ).Length > 35 )
		{
			var defaultPosition = pawn.TargetForward.Angles().WithPitch( 30 );
			Rotation = Rotation.Lerp( Rotation, Rotation.From( defaultPosition ), Time.Delta );
		}

		var spd = pawn.Velocity.WithZ( 0 ).Length / 350f;
		var fov = 82f.LerpTo( 92f, spd );

		FieldOfView = FieldOfView.LerpTo( fov, Time.Delta );
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
		Viewer = null;
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
