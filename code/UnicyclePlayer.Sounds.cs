using Sandbox;
using System;

internal partial class UnicyclePlayer
{

	private Sound? wheelSound;
	private float targetVolume;
	private float currentVolume;
	private string currentSoundName;

	[Event.Tick.Client]
	private void TickWheelSound()
	{
		var newSoundName = GetWheelSoundName();
		targetVolume = GetWheelVolume();

		if ( !wheelSound.HasValue || wheelSound.Value.Finished || currentSoundName != newSoundName )
		{
			wheelSound?.Stop();
			wheelSound = Sound.FromEntity( newSoundName, this );
			wheelSound.Value.SetVolume( targetVolume );
			currentVolume = targetVolume;
			currentSoundName = newSoundName;
		}

		currentVolume = currentVolume.LerpTo( targetVolume, 8f * Time.Delta );
		wheelSound.Value.SetVolume( currentVolume );

		if ( Controller is UnicycleController ctrl )
		{
			if ( ctrl.GroundEntity != null && !ctrl.PrevGrounded )
			{
				var snd = Sound.FromWorld( "land_solid_1", Position );
				var vol = Math.Clamp( Math.Abs( ctrl.PrevVelocity.z ) / 600f, .15f, 1f );
				snd.SetVolume( vol );
			}
		}
	}

	private float GetWheelVolume()
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

	private string GetWheelSoundName()
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

