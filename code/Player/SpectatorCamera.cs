
using Sandbox;
using System;
using System.Linq;
using System.Collections.Generic;

internal class SpectatorCamera : CameraMode
{
	private List<UfProp> viewblockers = new();
	private Vector3 CurrentPos = new Vector3();
	private Vector3 CurrentLook = Vector3.Forward;
	private float CurrentDist = 150.0f;

	static float Lerp( float a, float b, float f )
	{
		return a + f * (b - a);
	}

	public override void Update()
	{
		var bots = Client.All.Where( x => x.IsBot && x.Pawn is UnicyclePlayer && x.Pawn.LifeState == LifeState.Alive ).Select(x => x.Pawn as UnicyclePlayer).OrderByDescending( x => x.BotFitness ).ToArray();

		//Vector3 avgPos = new Vector3();
		////Vector3 avgLook = new Vector3();
		//float totalWeight = 0.0f;
		//for (int i = 0; i < bots.Length; i++ )
		//{
		//	//float weight = 1.0f - ((float)i / bots.Length) * 0.5f;
		//	float weight = 1.0f - ((float)i / bots.Length);
		//	weight = weight * weight;
		//	avgPos += bots[i].Position * weight;
		//	//avgLook += bots[i].Rotation.Forward.WithZ(0).Normal * weight;
		//	//avgLook += FitnessPath.Current.GetDirectionVector(bots[i].Position).WithZ(0).Normal;
		//	totalWeight += weight;
		//}

		//const float moveSpeed = 2.0f;

		//if ( totalWeight > 0.0f )
		//{
		//	avgPos /= totalWeight;
		//	//avgLook /= totalWeight;
		//	CurrentPos = Vector3.Lerp( CurrentPos, avgPos, Time.Delta * moveSpeed );
		//	//CurrentLook = Vector3.Lerp( CurrentLook, avgLook.Normal, Time.Delta * moveSpeed ).Normal;
		//}

		const float LerpSpeed = 2.0f;
		CurrentPos = Vector3.Lerp( CurrentPos, UnicycleFrenzy.Game.SpectatePos, LerpSpeed * Time.Delta );
		CurrentLook = Vector3.Lerp( CurrentLook, UnicycleFrenzy.Game.SpectateLook, LerpSpeed * Time.Delta ).Normal;
		//Vector3 currentPos = UnicycleFrenzy.Game.SpectatePos;
		//Vector3 currentLook = UnicycleFrenzy.Game.SpectateLook;

		// minimum distance
		float maxDist = 150.0f;
		foreach (var bot in bots)
		{
			float dist = bot.Position.Distance( CurrentPos );
			if ( dist > maxDist )
				maxDist = dist;
		}


		if (bots.Length > 0)
		{
			CurrentDist = Lerp( CurrentDist, maxDist, Time.Delta * UnicycleFrenzy.MoveSpeed );
		}
		//var pawn = Local.Pawn as UnicyclePlayer;

		//if ( pawn == null ) return;
		//if ( pawn.SpectateTarget.IsValid() ) pawn = pawn.SpectateTarget;

		ClearViewBlockers();
		//UpdateViewBlockers( pawn );

		//var viewangles = Input.Rotation.Angles();

		//// hack in sensitivity boost until we get sens slider for controllers
		//if ( Input.UsingController )
		//{
		//	var delta = Input.GetAnalog( InputAnalog.Look );
		//	viewangles += new Vector3( -delta.y * 6f, -delta.x * 6f, 0f );
		//}

		//viewangles.pitch = Math.Clamp( viewangles.pitch, -35f, 65f );

		//var targetRot = Rotation.From( viewangles );
		//var targetRot = Rotation.LookAt( lookDir, Vector3.Up );

		Vector3 lookOffset = Vector3.Down * 0.4f;

		var targetRot = Rotation.LookAt( (CurrentLook + lookOffset).Normal, Vector3.Up );
		//var center = pawn.Position + Vector3.Up * 80;
		var center = CurrentPos + Vector3.Up * 40;
		var distance = CurrentDist;
		var targetPos = center + targetRot.Forward * -distance;

		var tr = Trace.Ray( center, targetPos )
			//.Ignore( pawn )
			.Radius( 8 )
			.Run();

		var endpos = tr.EndPosition;

		if ( tr.Entity is UfProp ufp && ufp.NoCameraCollide )
			endpos = targetPos;

		Position = endpos;
		Rotation = targetRot;

		// controller constantly tries to center itself
		//if ( Input.UsingController && pawn.Velocity.WithZ( 0 ).Length > 35 )
		//{
		//	var defaultPosition = pawn.TargetForward.Angles().WithPitch( 30 );
		//	Rotation = Rotation.Lerp( Rotation, Rotation.From( defaultPosition ), Time.Delta );
		//}

		//var spd = pawn.Velocity.WithZ( 0 ).Length / 350f;
		//var fov = 82f.LerpTo( 92f, spd );

		//FieldOfView = FieldOfView.LerpTo( fov, Time.Delta );
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

		foreach ( var tr in traces )
		{
			if ( tr.Entity is not UfProp prop ) continue;
			prop.BlockingView = true;
			viewblockers.Add( prop );
		}
	}

}
