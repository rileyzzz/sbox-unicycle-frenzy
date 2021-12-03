using Sandbox;

internal partial class UnicyclePlayer
{

	// maybe client authoritative movement would suit us better

	[Net, Predicted]
	public float PedalPosition { get; set; } // -1 = left pedal down, 1 = right pedal down, 0 = flat
	[Net, Predicted]
	public Angles Tilt { get; set; }
	[Net, Predicted]
	public float TimeToReachTarget { get; set; }
	[Net, Predicted]
	public float PedalStartPosition { get; set; }
	[Net, Predicted]
	public float PedalTargetPosition { get; set; }
	[Net, Predicted]
	public TimeSince TimeSincePedalStart { get; set; }
	[Net, Predicted]
	public TimeSince TimeSinceJumpDown { get; set; }
	[Net, Predicted]
	public Rotation TargetForward { get; set; }

	private bool overrideRot;
	private Rotation rotOverride;

	public void Fall()
	{
		Host.AssertServer();

		Game.Current.DoPlayerSuicide( Client );
	}

	public void ResetMovement()
	{
		PedalPosition = default;
		Tilt = default;
		TimeToReachTarget = default;
		PedalStartPosition = default;
		PedalTargetPosition = default;
		TimeSincePedalStart = default;
		TargetForward = Rotation;
	}

	public override void BuildInput( InputBuilder input )
	{
		base.BuildInput( input );

		if ( !overrideRot ) return;
		input.ViewAngles = rotOverride.Angles();
		overrideRot = false;
	}

	[ClientRpc]
	private void SetRotationOnClient( Rotation rotation )
	{
		overrideRot = true;
		rotOverride = rotation;
	}

}

