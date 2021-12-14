using Sandbox;
using Sandbox.UI;
using System;

[UseTemplate]
internal class CourseTimer : Panel
{

	public string CourseTime 
	{ 
		get
		{
			if ( Local.Pawn is not UnicyclePlayer pl || pl.TimerState != TimerState.Live )
				return FormattedTime( 0 );

			return FormattedTime( pl.TimeSinceStart );
		} 
	}

	public string GameTime => FormattedTime( UnicycleFrenzy.Game.GameTime );

	public string MenuHotkey => Input.GetKeyWithBinding( "+iv_score" ) ?? "UNSET";

	public string FormattedTime( float seconds )
	{
		return TimeSpan.FromSeconds( seconds ).ToString( @"mm\:ss" );
	}

}

