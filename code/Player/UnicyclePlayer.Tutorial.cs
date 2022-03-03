using Sandbox;
using Sandbox.Component;
using System.Linq;

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

	[Event("collection.complete")]
	public void OnCollectionComplete( string collection )
	{
		if ( !Host.IsServer ) return;
		if ( !string.Equals( collection, "collection_tutorial" ) ) return;

		var ent = Entity.All.FirstOrDefault( x => x is DoorEntity && x.Name == "tut_door" ) as DoorEntity;
		if ( !ent.IsValid() ) return;

		ent.Open();
	}

	[Event("collection.reset")]
	public void OnCollectionReset( string collection )
	{
		if ( !Host.IsServer ) return;
		if ( !string.Equals( collection, "collection_tutorial" ) ) return;

		var ent = Entity.All.FirstOrDefault( x => x is DoorEntity && x.Name == "tut_door" ) as DoorEntity;
		if ( !ent.IsValid() ) return;

		ent.Close();
	}

}
