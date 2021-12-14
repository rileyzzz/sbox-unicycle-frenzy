using Sandbox;
using System;

internal partial class UnicyclePlayer
{

	[Net, Predicted]
	public TimerState TimerState { get; set; }
	[Net, Predicted]
	public TimeSince TimeSinceStart { get; set; }

	private float finishTime;

	public void StartCourse()
	{
		TimeSinceStart = 0;
		TimerState = TimerState.Live;
	}

	public void CompleteCourse()
	{
		finishTime = TimeSinceStart;
		TimerState = TimerState.Finished;
	}

}

public enum TimerState
{
	InStartZone,
	Live,
	Finished
}

