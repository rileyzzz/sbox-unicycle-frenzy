namespace Redzen.Random;

/// <summary>
/// A factory of Xoshiro256StarStarRandom instances.
/// </summary>
public class SandboxRandomFactory : IRandomSourceFactory
{
	readonly IRandomSeedSource _seedSource;

	#region Constructors

	/// <summary>
	/// Construct with a default seed source.
	/// </summary>
	public SandboxRandomFactory()
	{
		_seedSource = new DefaultRandomSeedSource();
	}

	/// <summary>
	/// Construct with the given seed source.
	/// </summary>
	/// <param name="seedSource">Random seed source.</param>
	public SandboxRandomFactory(
		IRandomSeedSource seedSource )
	{
		_seedSource = seedSource;
	}

	#endregion

	#region Public Methods

	public IRandomSource Create()
	{
		ulong seed = _seedSource.GetSeed();
		return new SandboxRandom( seed );
	}

	public IRandomSource Create( ulong seed )
	{
		return new SandboxRandom( seed );
	}

	#endregion
}
