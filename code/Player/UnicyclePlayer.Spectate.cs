using Sandbox;

internal partial class UnicyclePlayer
{

	[Net]
	public UnicyclePlayer SpectateTarget { get; set; }

	[Event.Tick.Server]
	private void EnsureSpectateTarget()
	{
		if ( !SpectateTarget.IsValid() || SpectateTarget == this )
			SpectateTarget = null;
	}

}
