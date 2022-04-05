using System;
using System.Collections.Generic;
using Sandbox;
using System.Linq;

public class UnicycleBot : Bot
{
	public UnicycleBrain Brain = BotManager.CreateBrain();

	int test = 0;
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

	double[] lastInputs = new double[BotManager.NumInputs];

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
		var inputs = GatherInputs();
		lastInputs = inputs;
		Brain.SetInputs( inputs );
		Brain.Execute();
		double[] outputs = Brain.GetOutputs();
		//Log.Info( $"NN outputs {string.Join( ",", outputs )}" );
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
		bool wantPedal = outputs[0] > 0.5f;
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
		builder.SetButton( InputButton.Jump, outputs[1] > 0.5f );

		// brake
		builder.SetButton( InputButton.SlotPrev, outputs[2] > 0.5f );

		// WASD/arrow keys
		// tilt pitch
		//builder.InputDirection.x = ((float)outputs[3] - 0.5f) * 2.0f;
		builder.InputDirection.x = ((float)outputs[3] - 0.5f) * 0.4f;
		//builder.InputDirection.y = ((float)outputs[4] - 0.5f) * 2.0f;
		//builder.InputDirection.y = ((float)outputs[4] - 0.5f) * 0.4f;

		//builder.SetButton( InputButton.Forward, outputs[4] > 0.5f );
		//builder.SetButton( InputButton.Back, outputs[5] > 0.5f );
		//builder.SetButton( InputButton.Left, outputs[6] > 0.5f );
		//builder.SetButton( InputButton.Right, outputs[7] > 0.5f );

		if ( Client.Pawn is not UnicyclePlayer player )
			return;

		float dir = ((float)outputs[4] - 0.5f) * 2.0f;
		builder.ViewAngles = new Angles( 0.0f, player.Rotation.Yaw() + dir * 80.0f, 0.0f );

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
		float fitness = Fitness;

		if ( Client.Pawn is not UnicyclePlayer player )
			return fitness;

		// distance to goal
		var checkpoints = Entity.All.OfType<Checkpoint>();
		Checkpoint start = checkpoints.FirstOrDefault( x => x.IsStart );
		Checkpoint end = checkpoints.FirstOrDefault( x => x.IsEnd );
		if (start == null || end == null)
		{
			Log.Warning("Couldn't find start or end checkpoint!");
			return fitness;
		}

		float dist = (end.Position - start.Position).Length;
		fitness += (1.0f - (lastGroundPosition - end.Position).Length / dist) * 10000.0f;

		fitness += avgVelocity / numVelocitySteps * 5.0f;

		return fitness;
	}

	public override void Tick()
	{
		if ( !Host.IsServer )
			return;

		if ( Client.Pawn is not UnicyclePlayer player )
			return;

		if ( player.LifeState == LifeState.Dead && !DeathMarked )
		{
			DeathMarked = true;
			DebugOverlay.Line( lastGroundPosition, lastGroundPosition + Vector3.Up * 20.0f, Color.Orange, 4.0f );
		}

		if (player.Velocity.Length > 0.5f)
			TimeSinceLastMovement = 0;

		if ( TimeSinceLastMovement > 1.0f )
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

	private double[] GatherInputs()
	{
		double[] inputs = new double[BotManager.NumInputs];
		if ( Client.Pawn is not UnicyclePlayer player )
			return inputs;

		// set up our inputs
		// distance to wall in front
		//const float searchDist = 1000.0f;
		//Vector3 start = player.Position + new Vector3(0.0f, 0.0f, 10.0f);
		//Vector3 searchDir = player.Rotation.Forward.WithZ( 0 ).Normal;
		//TraceResult tr = Trace.Ray( start, start + searchDir * searchDist )
		//	.WorldAndEntities()
		//	.Run();
		//DebugOverlay.TraceResult(tr);
		//inputs[0] = tr.Fraction;

		// player tilt left/ right
		//inputs[0] = player.Rotation.Roll() / 15.0f;
		// player tilt forward / back
		//inputs[1] = player.Rotation.Pitch() / 15.0f;

		// distance to next checkpoint
		//Checkpoint nearest = (Checkpoint)Entity.All.FirstOrDefault( x => x is Checkpoint );
		//float nearestDist = (nearest.Position - player.Position).Length;
		//foreach (var ent in Entity.All.OfType<Checkpoint>())
		//{
		//	float dist = (ent.Position - player.Position).Length;
		//	if (dist < nearestDist)
		//	{
		//		nearest = ent;
		//		nearestDist = dist;
		//	}
		//}
		//inputs[1] = nearestDist / 4000.0f;

		var checkpoints = Entity.All.OfType<Checkpoint>().OrderBy(x => (x.Position - player.Position).LengthSquared).ToArray();
		if (checkpoints.Length > 0)
		{
			Vector3 forward = player.Rotation.Forward;
			Vector3 checkpointDir = checkpoints[0].Position - player.Position;
			inputs[0] = Vector3.Dot(forward, checkpointDir);
		}


		// player speed
		inputs[1] = Math.Clamp(player.Velocity.Length / 400.0f, 0.0f, 1.0f);

		// distance to nearest gaps
		int center = 3;
		float[] searchAngles = new float[] {
			-90.0f,
			-40.0f,
			0.0f,
			40.0f,
			90.0f
		};

		float[] searchDists = new float[searchAngles.Length];
		float jumpDist = 1.0f;
		for ( int i = 0; i < searchAngles.Length; i++ )
		{
			Vector3 gapStart = player.Position + new Vector3( 0.0f, 0.0f, 20.0f );
			Angles gapAngle = player.Rotation.Angles();
			gapAngle.yaw += searchAngles[i];
			Rotation gapDir = Rotation.From( gapAngle );

			const float searchDist = 1000.0f;
			TraceResult wallTrace = Trace.Ray( gapStart, gapStart + gapDir.Forward * searchDist )
				.WorldAndEntities()
				.Run();
			
			const float testInterval = 8.0f;
			const int numSteps = 25;
			float testDistance = 0.0f;
			float searchHeight = 1000.0f;

			for ( int step = 0; step < numSteps; step++ )
			{
				if ( testDistance + testInterval >= wallTrace.Distance )
					break;

				testDistance += testInterval;

				Vector3 gapPos = gapStart + gapDir.Forward * testDistance;
				TraceResult gapTr = Trace.Ray( gapPos, gapPos + Vector3.Down * searchHeight )
					.WorldAndEntities()
					.Run();

				//DebugOverlay.TraceResult( gapTr );

				if ( !gapTr.Hit )
					break;
			}

			searchDists[i] = testDistance / (testInterval * numSteps);

			if (i == center)
			{
				// search for jumpable gap
				for ( int step = 0; step < numSteps; step++ )
				{
					if ( testDistance + testInterval >= wallTrace.Distance )
						break;

					testDistance += testInterval;

					Vector3 gapPos = gapStart + gapDir.Forward * testDistance;
					TraceResult gapTr = Trace.Ray( gapPos, gapPos + Vector3.Down * searchHeight )
						.WorldAndEntities()
						.Run();

					//DebugOverlay.TraceResult( gapTr );

					if ( gapTr.Hit )
					{
						jumpDist = testDistance / (testInterval * numSteps);
						break;
					}
				}
			}

			//DebugOverlay.Line( gapStart, gapStart + gapDir.Forward * testDistance, Color.Orange, 0.0f );

			// invert - closeness of gap instaed of distance to gap?
			//searchDists[i] = 1.0f - (testDistance / (testInterval * numSteps));
			
			//searchDists[i] = testDistance;
		}

		//Log.Info($"front search dist {searchDists[0]}");
		inputs[2] = searchDists[0];
		inputs[3] = searchDists[1];
		inputs[4] = searchDists[2];
		inputs[5] = searchDists[3];
		inputs[6] = searchDists[4];
		inputs[7] = jumpDist;

		inputs[8] = (Time.Now * 1.5f) % 1.0f;

		// copy speed and search dists from last
		for ( int i = 1; i < 8; i++ )
			inputs[i + 8] = (inputs[i] - lastInputs[i]) * Time.Delta;
			//inputs[i + 8] = lastInputs[i + 1];

		//Log.Info($"NN inputs {string.Join(",", Brain.Inputs)}");
		return inputs;
	}
}
