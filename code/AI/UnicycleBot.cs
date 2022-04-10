using System;
using System.Collections.Generic;
using Sandbox;
using System.Linq;

public class UnicycleBot : Bot, UnicycleAI.NetworkAgent
{
	//public UnicycleAI.UnicycleBrain Brain = BotManager.CreateBrain();

	float Fitness = 0.0f;
	Vector3 lastGroundPosition = new Vector3();
	public TimeSince TimeSinceLastMovement;
	public TimeSince TimeSinceStart;

	bool DeathMarked = false;

	float avgVelocity;
	int numVelocitySteps;
	bool Pedal = false;
	bool StoppedLast = false;
	bool PedaledLast = false;

	//double[] lastInputs = new double[BotManager.NumInputs];
	double[] Inputs = new double[BotManager.NumInputs];
	double[] Outputs = null;
	double[] Vision = null;

	public static UnicycleBot Create()
	{
		Host.AssertServer();

		return new UnicycleBot();
	}

	public UnicycleBot()
	{
		TimeSinceLastMovement = 0;
		TimeSinceStart = 0;
		avgVelocity = 0.0f;
		numVelocitySteps = 0;

		if ( Client.Pawn is UnicyclePlayer player )
			lastGroundPosition = player.Position;
	}

	public void Reset()
	{
		Fitness = 0.0f;
		TimeSinceLastMovement = 0;
		TimeSinceStart = 0;
		DeathMarked = false;
		avgVelocity = 0.0f;
		numVelocitySteps = 0;

		if ( Client.Pawn is UnicyclePlayer player )
			lastGroundPosition = player.Position;
	}

	public override void BuildInput( InputBuilder builder )
	{
		//var inputs = GatherInputs();
		//lastInputs = inputs;
		//Brain.SetInputs( inputs );
		//Brain.Execute();
		//double[] outputs = Brain.GetOutputs();

		if ( Outputs == null )
			return;

		//Log.Info( $"NN outputs {string.Join( ",", Outputs )}" );
		// Outputs:
		// LMB
		// RMB
		// space
		// ctrl
		// arrow keys
		// look yaw

		// left pedal
		//if ( outputs[0] > 0.5f )
		//	Log.Info("left pedal");

		//if ( outputs[1] > 0.5f )
		//	Log.Info( "right pedal" );
		// automatically alternate pedal, there aren't really circumstances where you'd pedal the same twice
		//bool wantPedal = Outputs[0] > 0.25f;
		//bool wantPedal = (Time.Now * 1.5f) % 1.0f > 0.5f;
		bool wantPedal = (Time.Now * 1.0f) % 1.0f > 0.5f;
		//bool wantPedal = Time.Now * 1.5f % 1.0f > 0.5f;
		if (wantPedal)
			PedaledLast = true;

		if (!wantPedal && PedaledLast)
		{
			PedaledLast = false;
			Pedal = !Pedal;
		}

		builder.SetButton( Pedal ? InputButton.Attack1 : InputButton.Attack2, wantPedal );
		builder.SetButton( Pedal ? InputButton.Attack2 : InputButton.Attack1, false );


		//builder.SetButton( InputButton.Attack1, outputs[0] > 0.5f );

		// right pedal
		//builder.SetButton( InputButton.Attack2, outputs[1] > 0.5f );

		// jump
		builder.SetButton( InputButton.Jump, Outputs[0] > 0.5f );

		// brake
		builder.SetButton( InputButton.SlotPrev, Outputs[1] > 0.5f );

		// WASD/arrow keys
		// tilt pitch
		//builder.InputDirection.x = ((float)outputs[3] - 0.5f) * 2.0f;
		//builder.InputDirection.x = ((float)outputs[3] - 0.5f) * 0.4f;
		builder.InputDirection.x = ((float)Outputs[2] - 0.5f) * 1.0f;

		//builder.InputDirection.y = ((float)outputs[4] - 0.5f) * 2.0f;
		//builder.InputDirection.y = ((float)outputs[4] - 0.5f) * 0.4f;

		//builder.SetButton( InputButton.Forward, outputs[4] > 0.5f );
		//builder.SetButton( InputButton.Back, outputs[5] > 0.5f );
		//builder.SetButton( InputButton.Left, outputs[6] > 0.5f );
		//builder.SetButton( InputButton.Right, outputs[7] > 0.5f );

		if ( Client.Pawn is not UnicyclePlayer player )
			return;

		//Log.Info($"output dir {Outputs[3]}");
		//float dir = ((float)Outputs[3] - 0.5f) * 2.0f;
		//float dir = ((float)Outputs[3] - 0.5f) * 1.0f;
		float dir = (float)Outputs[3] * 1.0f;
		builder.ViewAngles = new Angles( 0.0f, player.Rotation.Yaw() - dir * 80.0f, 0.0f );

		Vector3 eyePos = player.Position + new Vector3(0.0f, 0.0f, 72.0f);
		//DebugOverlay.Line( eyePos, eyePos + Rotation.From(builder.ViewAngles).Forward * 50.0f, Color.Blue, 0.01f );
		//test++;

		//bool left = false;
		//if ( test % 50 < 25 )
		//	left = true;

		//builder.SetButton( left ? InputButton.Attack1 : InputButton.Attack2, true );
		//builder.SetButton( left ? InputButton.Attack2 : InputButton.Attack1, false );



		//player.EyeRotation = Rotation.From(0.0f, 0.0f, 45.0f);
		//builder.InputDirection = new Vector3(0.0f, 0.0f, 20.0f);
		//builder.ViewAngles = new Angles(0.0f, 45.0f, 0.0f);

		//builder.OriginalViewAngles = new Angles(0.0f, 0.0f, 45.0f);
	}

	public float GetFitness()
	{
		return Fitness;
	}

	private void UpdateFitness()
	{
		float fitness = 0.0f;

		if ( Client.Pawn is UnicyclePlayer player )
		{
			// distance to goal
			//var checkpoints = Entity.All.OfType<Checkpoint>();
			//Checkpoint start = checkpoints.FirstOrDefault( x => x.IsStart );
			//Checkpoint end = checkpoints.FirstOrDefault( x => x.IsEnd );
			//if (start == null || end == null)
			//{
			//	Log.Warning("Couldn't find start or end checkpoint!");
			//	return fitness;
			//}
			fitness += FitnessPath.Current.GetKeyAlongPath( lastGroundPosition ) * 1000.0f;

			//float dist = (end.Position - start.Position).Length;
			//fitness += (1.0f - (lastGroundPosition - end.Position).Length / dist) * 10000.0f;

			//fitness += avgVelocity / numVelocitySteps * 5.0f;
		}
		Fitness = fitness;
	}

	private void UpdateInputs()
	{
		if ( Client.Pawn is not UnicyclePlayer player )
			return;


		Vector3 direction = FitnessPath.Current.GetDirectionVector( player.Position );
		float targetDir = (Rotation.LookAt( direction.WithZ( 0 ) ).Yaw() - player.Rotation.Yaw()).NormalizeDegrees() / 180.0f;
		if ( targetDir > 1.0f )
			targetDir = targetDir - 2.0f;
		Inputs[0] = targetDir;


		// player speed
		Inputs[1] = Math.Clamp( player.Velocity.Length / 400.0f, 0.0f, 1.0f );

		//Inputs[2] = (Time.Now * 1.5f) % 1.0f;

		if ( Vision != null )
			Vision.CopyTo( Inputs, 2 );

		// distance to nearest gaps
		//int center = 3;
		//float[] searchAngles = new float[] {
		//	-90.0f,
		//	-40.0f,
		//	0.0f,
		//	40.0f,
		//	90.0f
		//};

		//float[] searchDists = new float[searchAngles.Length];
		//float jumpDist = 1.0f;
		//for ( int i = 0; i < searchAngles.Length; i++ )
		//{
		//	Vector3 gapStart = player.Position + new Vector3( 0.0f, 0.0f, 20.0f );
		//	Angles gapAngle = player.Rotation.Angles();
		//	gapAngle.yaw += searchAngles[i];
		//	Rotation gapDir = Rotation.From( gapAngle );

		//	const float searchDist = 1000.0f;
		//	TraceResult wallTrace = Trace.Ray( gapStart, gapStart + gapDir.Forward * searchDist )
		//		.WorldAndEntities()
		//		.Run();

		//	const float testInterval = 8.0f;
		//	const int numSteps = 25;
		//	float testDistance = 0.0f;
		//	float searchHeight = 1000.0f;

		//	for ( int step = 0; step < numSteps; step++ )
		//	{
		//		if ( testDistance + testInterval >= wallTrace.Distance )
		//			break;

		//		testDistance += testInterval;

		//		Vector3 gapPos = gapStart + gapDir.Forward * testDistance;
		//		TraceResult gapTr = Trace.Ray( gapPos, gapPos + Vector3.Down * searchHeight )
		//			.WorldAndEntities()
		//			.Run();

		//		//DebugOverlay.TraceResult( gapTr );

		//		if ( !gapTr.Hit )
		//			break;
		//	}

		//	searchDists[i] = testDistance / (testInterval * numSteps);

		//	if (i == center)
		//	{
		//		// search for jumpable gap
		//		for ( int step = 0; step < numSteps; step++ )
		//		{
		//			if ( testDistance + testInterval >= wallTrace.Distance )
		//				break;

		//			testDistance += testInterval;

		//			Vector3 gapPos = gapStart + gapDir.Forward * testDistance;
		//			TraceResult gapTr = Trace.Ray( gapPos, gapPos + Vector3.Down * searchHeight )
		//				.WorldAndEntities()
		//				.Run();

		//			//DebugOverlay.TraceResult( gapTr );

		//			if ( gapTr.Hit )
		//			{
		//				jumpDist = testDistance / (testInterval * numSteps);
		//				break;
		//			}
		//		}
		//	}

		//	//DebugOverlay.Line( gapStart, gapStart + gapDir.Forward * testDistance, Color.Orange, 0.0f );

		//	// invert - closeness of gap instaed of distance to gap?
		//	//searchDists[i] = 1.0f - (testDistance / (testInterval * numSteps));

		//	//searchDists[i] = testDistance;
		//}

		//inputs[2] = searchDists[0];
		//inputs[3] = searchDists[1];
		//inputs[4] = searchDists[2];
		//inputs[5] = searchDists[3];
		//inputs[6] = searchDists[4];
		//inputs[7] = jumpDist;


		// copy speed and search dists from last
		//for ( int i = 1; i < 8; i++ )
		//inputs[i + 8] = (inputs[i] - lastInputs[i]) * Time.Delta;

		//Log.Info($"NN inputs {string.Join(",", Brain.Inputs)}");
	}

	public override void Tick()
	{
		if ( !Host.IsServer )
			return;

		if ( Client.Pawn is not UnicyclePlayer player )
			return;

		Vision = GatherVision();
		UpdateInputs();
		UpdateFitness();
		player.BotVision = new List<double>( Vision );

		player.BotFitness = GetFitness();

		if ( player.LifeState == LifeState.Dead && !DeathMarked )
		{
			DeathMarked = true;
			DebugOverlay.Line( lastGroundPosition, lastGroundPosition + Vector3.Up * 20.0f, Color.Orange, 4.0f );
		}

		if (player.Velocity.Length > 0.5f)
			TimeSinceLastMovement = 0;

		if ( TimeSinceLastMovement > 2.0f )
		{
			DamageInfo dmg = new DamageInfo();
			dmg.Damage = 1000.0f;
			player.TakeDamage( dmg );
			return;
		}

		// if we're just moving in circles
		if ( TimeSinceStart > 15.0f && GetFitness() < 1000.0f )
		{
			DamageInfo dmg = new DamageInfo();
			dmg.Damage = 1000.0f;
			player.TakeDamage( dmg );
			return;
		}

		// if we've lasted an absurdly long time, abort
		if ( TimeSinceStart > 80.0f )
		{
			DamageInfo dmg = new DamageInfo();
			dmg.Damage = 1000.0f;
			player.TakeDamage( dmg );
			return;
		}

		//if ( TimeSinceStart > 200.0f )

		if ( player.LifeState == LifeState.Alive && player.GroundEntity != null )
		{
			//Fitness += 10.0f;
			//Fitness += player.Velocity.Length * 0.1f;
			avgVelocity += player.Velocity.Length;
			numVelocitySteps++;

			lastGroundPosition = player.Position;
		}
	}

	Checkpoint GetNextCheckpoint()
	{
		if ( Client.Pawn is not UnicyclePlayer player )
			return null;

		var checkpoints = Entity.All.OfType<Checkpoint>().Where(x => !player.Checkpoints.Contains(x));
		
		Checkpoint nearest = checkpoints.FirstOrDefault();
		if ( nearest == null )
			return null;

		float nearestDist = (nearest.Position - player.Position).Length;
		foreach ( var ent in checkpoints )
		{
			float dist = (ent.Position - player.Position).Length;
			if ( dist < nearestDist )
			{
				nearest = ent;
				nearestDist = dist;
			}
		}
		return nearest;
	}

	public bool IsActive()
	{
		if ( Client.Pawn is not UnicyclePlayer player )
			return false;

		return player.LifeState == LifeState.Alive;
	}

	public double[] GetInputs()
	{
		return Inputs;
	}

	public void SetOutputs( double[] outputs )
	{
		//Log.Info($"setting outputs for {Client.Name}");
		Outputs = outputs;
	}

	public const int VisionRange = 10;
	const float VisionScale = 40.0f;

	public static Dictionary<Vector2, float> VisionCache = new();

	double VisionTrace(Vector3 pos)
	{
		// quantize
		pos = new Vector3((int)pos.x, (int)pos.y, pos.z);

		if ( VisionCache.TryGetValue( new Vector2( pos ), out float value ) )
			return value;

		Vector3 startPos = pos + Vector3.Up * 72.0f;
		float searchHeight = 800.0f;
		TraceResult tr = Trace.Ray( startPos, startPos + Vector3.Down * searchHeight )
			.WorldAndEntities()
			.Run();

		//DebugOverlay.TraceResult( tr );

		//float val = 1.0f - tr.Fraction;
		float val = tr.Fraction;
		VisionCache[pos] = val;
		return val;
	}

	double[] GatherVision()
	{
		double[] vision = new double[(VisionRange + 1) * (VisionRange + 1)];
		if ( Client.Pawn is not UnicyclePlayer player )
			return vision;

		for (int y = 0; y <= VisionRange; y++ )
		{
			for (int x = 0; x <= VisionRange; x++ )
			{
				int idx = y * (VisionRange + 1) + x;

				Vector3 pos = new Vector3( (x - VisionRange / 2) * VisionScale, (y - VisionRange / 2) * VisionScale, 0 );
				Transform t = new Transform( player.Position, Rotation.FromYaw( player.Rotation.Yaw() ) );
				//Transform t = new Transform( player.Position );
				Vector3 worldPos = t.PointToWorld(pos);

				vision[idx] = VisionTrace(worldPos);
			}
		}

		return vision;
	}
}
