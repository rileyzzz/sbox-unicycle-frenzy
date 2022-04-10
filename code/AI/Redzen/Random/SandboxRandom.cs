
using System;
using Sandbox;
namespace Redzen.Random;

public sealed class SandboxRandom : RandomSourceBase, IRandomSource
{
	System.Random rand = new System.Random();

	public SandboxRandom()
	{
	}

	public SandboxRandom(ulong seed)
	{
		Reinitialise(seed);
	}

	public void Reinitialise( ulong seed )
	{
		rand = new System.Random( (int)seed );
	}

	public override void NextBytes( byte[] buffer )
	{
		rand.NextBytes( buffer );
	}

	protected override ulong NextULongInner()
	{
		return (ulong)rand.NextInt64();
	}

}
