using Sandbox;
using System;

internal partial class UnicyclePlayer
{

	private Sound? rollingSound;
	private float targetVolume;
	private float currentVolume;
	private string currentSoundName;

	private void SimulateSound()
	{
		if ( IsServer ) return;

		using var _ = Prediction.Off();

		var newSoundName = GetSoundName();
		targetVolume = GetTargetVolume();

		if ( !rollingSound.HasValue || rollingSound.Value.Finished || currentSoundName != newSoundName )
		{
			rollingSound?.Stop();
			rollingSound = Sound.FromEntity( newSoundName, this );
			rollingSound.Value.SetVolume( targetVolume );
			currentVolume = targetVolume;
			currentSoundName = newSoundName;
		}

		currentVolume = currentVolume.LerpTo( targetVolume, 8f * Time.Delta );
		rollingSound.Value.SetVolume( currentVolume );
	}

	private float GetTargetVolume()
	{
		if ( GroundEntity == null || Health <= 0 )
		{
			return 0f;
		}
		else
		{
			var result = 1f * Velocity.WithZ( 0 ).Length / 900f;
			return Math.Clamp( result, .1f, 1f );
		}
	}

	private string GetSoundName()
	{
		if ( Controller is not UnicycleController ctrl )
			return "rolling_dirt";

		return ctrl.GroundSurface switch
		{
			"dirt" => "rolling_dirt",
			_ => "rolling_road",
		};
	}

}

