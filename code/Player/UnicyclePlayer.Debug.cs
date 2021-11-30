using Sandbox;

internal partial class UnicyclePlayer
{

	[ConVar.Replicated( "uf_debug_playground_ramp" )]
	public static bool DebugRamp { get; set; } = false;

	[Net, Predicted]
	public TimeSince TimeSinceDebugRamp { get; set; }

	[Event.Tick]
	private void OnTick()
	{
		if ( DebugRamp && TimeSinceDebugRamp > 1f )
		{
			TimeSinceDebugRamp = 0f;
			Position = new Vector3( 3550.32f, 693.15f - 90.46f, -122f );
			Rotation = Rotation.From( -1.08f, 1.41f, -0.00f );
			Velocity = Rotation.Forward.WithZ( 0 ) * 1500f;
		}
	}

}

