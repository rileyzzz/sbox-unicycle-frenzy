using Facepunch.Customization;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

internal partial class SpectatorPlayer : Sandbox.Player
{

	public override void Respawn()
	{
		base.Respawn();

		EnableDrawing = false;
		EnableAllCollisions = false;

		CameraMode = new FirstPersonCamera();
		Controller = new NoclipController();

		//var cp = Checkpoints.LastOrDefault();
		//if ( !cp.IsValid() )
		//{
		//	cp = Entity.All.FirstOrDefault( x => x is Checkpoint c && c.IsStart ) as Checkpoint;
		//	if ( cp == null ) return;
		//}
		var cp = Entity.All.FirstOrDefault( x => x is Checkpoint c && c.IsStart ) as Checkpoint;
		cp.GetSpawnPoint( out Vector3 position, out Rotation rotation );

		Position = position + Vector3.Up * 5;
		Rotation = rotation;
		Velocity = Vector3.Zero;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();
	}

	public override void OnKilled()
	{
		base.OnKilled();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( IsServer )
		{
			//Vector3 direction = FitnessPath.Current.GetDirectionVector(Position);
			////float test = Rotation.LookAt( direction.WithZ( 0 ) ).Distance( Rotation.FromYaw( EyeRotation.Yaw() ) );
			//float test = (Rotation.LookAt( direction.WithZ( 0 ) ).Yaw() - EyeRotation.Yaw()).NormalizeDegrees() / 180.0f;
			//if ( test > 1.0f )
			//	test = test - 2.0f;

			//Log.Info($"relative rot {test}");

			//Vector3 startPos = Position + Vector3.Down * 36.0f;
			//DebugOverlay.Line( startPos, startPos + direction * 200.0f, Color.Blue );

			//float key = FitnessPath.Current.GetKeyAlongPath( Position );
			//Vector3 pos = FitnessPath.Current.GetPointAlongPath( key );
			//float dist = (Position - pos).Length;
			//Vector3 next = FitnessPath.Current.GetPointAlongPath( key + dist / 800.0f );

			////Vector3 dir = FitnessPath.Current.GetDirectionAlongPath( key );
			//Vector3 dir = (next - pos).Normal;

			//Vector3 start = Position + Vector3.Down * 72.0f;
			//DebugOverlay.Line( start, start + dir * 100.0f, Color.Blue );

			//FitnessPath.Current.DrawPath(1);
			//float key = FitnessPath.Current.GetKeyAlongPath( Position );
			//int segment = FitnessPath.Current.GetClosestSegment( Position );
			//Log.Info($"current segment {segment} key {key}");

			//float dist = FitnessPath.GetSegmentDistance(FitnessPath.Current.GetNodePos( 0 ), FitnessPath.Current.GetNodePos( 1 ), Position);
			//Log.Info($"Curve dist {dist}");

			//Log.Info($"fitness {FitnessPath.Current.GetKeyAlongPath( Position ) * 1000.0f}" );
		}
	}

}

