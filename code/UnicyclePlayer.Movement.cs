using Sandbox;
using System.Numerics;

internal partial class UnicyclePlayer
{

	// maybe client authoritative movement would suit us better

	[Net, Predicted]
	public float PedalPosition { get; set; }
	[Net, Predicted]
	public Angles Lean { get; set; } // -1 = left pedal down, 1 = right pedal down, 0 = flat
	[Net, Predicted]
	public float TimeToReachTarget { get; set; }
	[Net, Predicted]
	public float PedalStartPosition { get; set; }
	[Net, Predicted]
	public float PedalTargetPosition { get; set; }
	[Net, Predicted]
	public TimeSince TimeSincePedalStart { get; set; }
	[Net, Predicted]
	public Rotation TargetForward { get; set; }

	private Rotation rotOverride;

	public void Fall()
	{
		Host.AssertServer();

		Game.Current.DoPlayerSuicide( Client );
	}

	public void ResetMovement()
	{
		PedalPosition = default;
		Lean = default;
		TimeToReachTarget = default;
		PedalStartPosition = default;
		PedalTargetPosition = default;
		TimeSincePedalStart = default;
		TargetForward = default;
	}

	public override void BuildInput( InputBuilder input )
	{
		base.BuildInput( input );

		if ( rotOverride == Quaternion.Identity ) return;
		input.ViewAngles = rotOverride.Angles();
		rotOverride = Quaternion.Identity;
	}

	[ClientRpc]
	private void SetRotationOnClient( Rotation rotation )
	{
		rotOverride = rotation;
	}

}

