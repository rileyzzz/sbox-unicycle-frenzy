using Sandbox;

internal class UnicycleCamera : Camera
{

	public override void Update()
	{
		var pawn = Local.Pawn as AnimEntity;

		if ( pawn == null ) return;

		var center = pawn.Position + Vector3.Up * 64;
		var distance = 130.0f * pawn.Scale;
		var targetPos = center + Input.Rotation.Forward * -distance;

		var tr = Trace.Ray( center, targetPos )
			.Ignore( pawn )
			.Radius( 8 )
			.Run();

		Position = tr.EndPos;
		Rotation = Input.Rotation;

		FieldOfView = 70;

		Viewer = null;
	}

}
