using Sandbox;
using System.Collections.Generic;
using System.Linq;

internal class UnicycleCamera : Camera
{

	private List<UfProp> viewblockers = new();

	public override void Update()
	{
		var pawn = Local.Pawn as UnicyclePlayer;

		if ( pawn == null ) return;
		if ( pawn.SpectateTarget.IsValid() ) pawn = pawn.SpectateTarget;

		ClearViewBlockers();
		UpdateViewBlockers( pawn );

		var center = pawn.Position + Vector3.Up * 80;
		var distance = 150.0f * pawn.Scale;
		var targetPos = center + Input.Rotation.Forward * -distance;

		var tr = Trace.Ray( center, targetPos )
			.Ignore( pawn )
			.Radius( 8 )
			.Run();

		var endpos = tr.EndPos;

		if ( tr.Entity is UfProp ufp )
		{
			if ( ufp.NoCameraCollide )
				endpos = targetPos;
		}

		Position = endpos;
		Rotation = Input.Rotation;

		var rot = pawn.Rotation.Angles() * .015f;
		rot.yaw = 0;

		Rotation *= Rotation.From( rot );

		var spd = pawn.Velocity.WithZ( 0 ).Length / 350f;
		var fov = 82f.LerpTo( 92f, spd );

		FieldOfView = FieldOfView.LerpTo( fov, Time.Delta );

		Viewer = null;
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
		var traces = Trace.Sphere( 3f, CurrentView.Position, pawn.Position ).RunAll();

		if ( traces == null ) return;

		foreach(var tr in traces )
		{
			if ( tr.Entity is not UfProp prop ) continue;
			prop.BlockingView = true;
			viewblockers.Add( prop );
		}
	}

}
