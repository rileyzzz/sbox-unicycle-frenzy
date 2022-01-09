using Sandbox;
using System.Linq;

internal partial class UnicyclePlayer
{

	[Net, Predicted]
	public TimerState TimerState { get; set; }
	[Net, Predicted]
	public TimeSince TimeSinceStart { get; set; }
	[Net, Change]
	public float BestTime { get; set; } = defaultBestTime;

	public bool CourseIncomplete => BestTime == defaultBestTime;

	private const float defaultBestTime = 3600f; // easier to check for this than sorting out 0/default

	public void EnterStartZone()
	{
		ResetTimer();
	}

	public void StartCourse()
	{
		TimeSinceStart = 0;
		TimerState = TimerState.Live;

		AddAttempts();
	}

	public void CompleteCourse()
	{
		TimerState = TimerState.Finished;

		if( IsServer )
		{
			ClearCheckpoints();

			var formattedTime = CourseTimer.FormattedTimeMsf( TimeSinceStart );

			if ( TimeSinceStart < BestTime )
			{
				if( CourseIncomplete )
				{
					UfChatbox.AddCustom( To.Everyone, $"{Client.Name} completed the course in {formattedTime}", "timer-msg" );
				}
				else
				{
					var improvement = CourseTimer.FormattedTimeMsf( BestTime - TimeSinceStart );
					UfChatbox.AddCustom( To.Everyone, $"{Client.Name} completed the course in {formattedTime}, improving by {improvement}!", "timer-msg" );
				}

				BestTime = TimeSinceStart;
			}

			Celebrate();
		}
	}

	public void ResetTimer()
	{
		TimerState = TimerState.InStartZone;
		TimeSinceStart = 0;

		if ( IsServer ) ClearCheckpoints();
	}

	public void ClearCheckpoints()
	{
		Host.AssertServer();

		Checkpoints.Clear();
	}

	public void TrySetCheckpoint( Checkpoint checkpoint )
	{
		Host.AssertServer();

		if ( Checkpoints.Contains( checkpoint ) ) return;
		Checkpoints.Add( checkpoint );
	}

	public void GotoBestCheckpoint()
	{
		Host.AssertServer();

		var cp = Checkpoints.LastOrDefault();
		if ( !cp.IsValid() )
		{
			cp = Entity.All.FirstOrDefault( x => x is Checkpoint c && c.IsStart ) as Checkpoint;
			if ( cp == null ) return;
		}

		cp.GetSpawnPoint( out Vector3 position, out Rotation rotation );

		Position = position + Vector3.Up * 5;
		Rotation = rotation;
		Velocity = Vector3.Zero;

		SetRotationOnClient( Rotation );
		ResetInterpolation();
		ResetMovement();
	}

	private void OnBestTimeChanged()
	{
		if ( !IsLocalPawn ) return;
		MapStats.Local.SetBestTime( BestTime );
	}

	[ClientRpc]
	private void AddAttempts()
	{
		if ( !IsLocalPawn ) return;
		MapStats.Local.AddAttempts();
	}

	[ClientRpc]
	private void Celebrate()
	{
		if ( !IsLocalPawn ) return;

		Particles.Create( "particles/finish/finish_effect.vpcf" );
		Sound.FromScreen( "course.complete" );
	}

}

public enum TimerState
{
	InStartZone,
	Live,
	Finished
}

