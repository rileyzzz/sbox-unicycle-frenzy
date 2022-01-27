using Sandbox;
using System.Linq;

internal class UnicycleCamera : Camera
{

	public override void Update()
	{
		var pawn = Local.Pawn as UnicyclePlayer;

		if ( pawn == null ) return;
		if ( pawn.SpectateTarget.IsValid() ) pawn = pawn.SpectateTarget;

		var center = pawn.Position + Vector3.Up * 80;
		var distance = 150.0f * pawn.Scale;
		var targetPos = center + Input.Rotation.Forward * -distance;

		var tr = Trace.Ray( center, targetPos )
			.Ignore( pawn )
			.Radius( 8 )
			.Run();

		Position = tr.EndPos;
		Rotation = Input.Rotation;

		FieldOfView = 80;

		Viewer = null;
	}

}
