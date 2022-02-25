using Sandbox;
using Sandbox.Component;

partial class UnicyclePlayer
{

	[Net]
	public TutorialTrigger.InputActions DisplayedAction { get; set; }
	[Net]
	public bool PerfectPedalGlow { get; set; }

	[Event.Frame]
	private void OnFrame()
	{
		if ( !Unicycle.IsValid() ) return;

		// pedal glow
		var glowLeftPedal = PerfectPedalGlow; 
		var glowRightPedal = PerfectPedalGlow;

		if ( Controller is UnicycleController c )
		{
			c.CanPedalBoost( out bool leftPedal, out bool rightPedal );
			glowLeftPedal = glowLeftPedal && leftPedal;
			glowRightPedal = glowRightPedal && rightPedal;
		}

		if( Unicycle.DisplayedLeftPedal.IsValid() && Unicycle.DisplayedRightPedal.IsValid() )
		{
			var leftGlow = Unicycle.DisplayedLeftPedal.Components.GetOrCreate<Glow>();
			var rightGlow = Unicycle.DisplayedRightPedal.Components.GetOrCreate<Glow>();
			leftGlow.Active = glowLeftPedal;
			leftGlow.Color = Color.Green;
			rightGlow.Active = glowRightPedal;
			rightGlow.Color = Color.Green;
		}
	}

}
