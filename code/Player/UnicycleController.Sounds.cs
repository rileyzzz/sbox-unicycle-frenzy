using Sandbox;
using System;

internal partial class UnicycleController
{

	private Sound? rollingSound;
	private float rollingSoundTargetVol;
	private float rollingSoundVolume;
	private string rollingSoundName;

	public override void OnEvent( string name )
	{
		base.OnEvent( name );

		switch ( name )
		{
			case "land":
				var snd = Sound.FromWorld( "unicycle.land.default", Position );
				var vol = Math.Clamp( Math.Abs( PrevVelocity.z ) / 600f, .15f, 1f );
				snd.SetVolume( vol );
				break;
			case "fall":
				Sound.FromWorld( "unicycle.crash.default", Position );
				rollingSound?.Stop();
				break;
			case "pedal":
				Sound.FromWorld( "unicycle.pedal", Position );
				break;
		}
	}

	private void DoRollingSound()
	{
		var newRollingSoundName = GetRollingSoundName();
		rollingSoundTargetVol = GetRollingVolume();

		if ( !rollingSound.HasValue || rollingSound.Value.Finished || rollingSoundName != newRollingSoundName )
		{
			rollingSound?.Stop();
			rollingSound = Sound.FromEntity( newRollingSoundName, Pawn );
			rollingSound.Value.SetVolume( rollingSoundTargetVol );
			rollingSoundVolume = rollingSoundTargetVol;
			rollingSoundName = newRollingSoundName;
		}

		rollingSoundVolume = rollingSoundVolume.LerpTo( rollingSoundTargetVol, 8f * Time.Delta );
		rollingSound.Value.SetVolume( rollingSoundVolume );
	}

	private float GetRollingVolume()
	{
		if ( GroundEntity == null ) return 0f;
		if ( Velocity.WithZ( 0 ).Length < 25f ) return 0f;
		if ( Pawn.Health <= 0 ) return 0f;

		return .08f.LerpTo( 1f, Velocity.WithZ( 0 ).Length / 500f );
	}

	private string GetRollingSoundName()
	{
		return GroundSurface switch
		{
			"dirt" => "unicycle.rolling.dirt",
			_ => "unicycle.rolling.default",
		};
	}

}
