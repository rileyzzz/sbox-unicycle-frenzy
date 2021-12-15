using Sandbox;

internal partial class UnicyclePlayer
{

	[Net, Predicted]
	public TimerState TimerState { get; set; }
	[Net, Predicted]
	public TimeSince TimeSinceStart { get; set; }
	[Net]
	public float BestTime { get; set; } = float.MaxValue;

	public void StartCourse()
	{
		TimeSinceStart = 0;
		TimerState = TimerState.Live;
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
				if( BestTime == float.MaxValue )
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
		}
	}

}

public enum TimerState
{
	InStartZone,
	Live,
	Finished
}

