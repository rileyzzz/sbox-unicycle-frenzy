using Sandbox;
using Sandbox.UI;

[UseTemplate]
internal class CourseTimer : Panel
{

	public string TimerTime => GetTimerTime();

	private string GetTimerTime()
	{
		if ( Local.Pawn is not UnicyclePlayer pl || pl.TimerState != TimerState.Live ) 
			return "Not Live";

		return pl.FormattedTime();
	}

}

