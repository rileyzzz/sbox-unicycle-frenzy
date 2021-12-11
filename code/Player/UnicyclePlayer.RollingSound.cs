using Sandbox;

internal partial class UnicyclePlayer
{

	private Sound? rollingSound;
	private float rollingSoundTargetVol;
	private float rollingSoundVolume;
	private string rollingSoundName;

	[Event.Tick.Client]
	private void DoRollingSound()
	{
		if( Health <= 0 || Controller is not UnicycleController )
		{
			rollingSound?.Stop();
			rollingSound = null;
			return;
		}

		var newRollingSoundName = GetRollingSoundName();
		rollingSoundTargetVol = GetRollingVolume();

		if ( !rollingSound.HasValue || rollingSound.Value.Finished || rollingSoundName != newRollingSoundName )
		{
			rollingSound?.Stop();
			rollingSound = Sound.FromEntity( newRollingSoundName, this );
			rollingSound?.SetVolume( rollingSoundTargetVol );
			rollingSoundVolume = rollingSoundTargetVol;
			rollingSoundName = newRollingSoundName;
		}

		rollingSoundVolume = rollingSoundVolume.LerpTo( rollingSoundTargetVol, 8f * Time.Delta );
		rollingSound?.SetVolume( rollingSoundVolume );
	}

	private float GetRollingVolume()
	{
		if ( GroundEntity == null ) return 0f;
		if ( Velocity.WithZ( 0 ).Length < 25f ) return 0f;
		if ( Health <= 0 ) return 0f;

		return .08f.LerpTo( 1f, Velocity.WithZ( 0 ).Length / 500f );
	}

	private string GetRollingSoundName()
	{
		var surface = "default";
		if( Controller is UnicycleController ctrl )
		{
			surface = ctrl.GroundSurface;
		}

		return surface switch
		{
			"dirt" => "unicycle.rolling.dirt",
			_ => "unicycle.rolling.default",
		};
	}

}
