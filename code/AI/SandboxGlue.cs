
public static class Debug
{
	public static void Assert( bool condition )
	{
		if (!condition)
		{
			Sandbox.Assert.True(false);
		}
	}

	public static void Assert( bool condition, string message )
	{
		if ( !condition )
		{
			Sandbox.Assert.True( false );
			Log.Error(message);
		}
	}

	//public static void Assert( bool condition, ref AssertInterpolatedStringHandler message )
	//{
	//}

	public static void Assert( bool condition, string message, string detailMessage )
	{
		if ( !condition )
		{
			Sandbox.Assert.True( false );
			Log.Error( message );
			Log.Error( detailMessage );
		}
	}

	//public static void Assert( bool condition, ref AssertInterpolatedStringHandler message, ref AssertInterpolatedStringHandler detailMessage )
	//{
	//}

	public static void Assert( bool condition, string message, string detailMessageFormat, params object[] args )
	{
		if ( !condition )
		{
			Sandbox.Assert.True( false );
			Log.Error( message );
			//Log.Error( detailMessage );
		}
	}

	public static void WriteLine( object value )
	{
		Log.Info( value );
	}

	public static void WriteLine( string message )
	{
		Log.Info( message );
	}

	public static void Write( object value )
	{
		Log.Info( value );
	}

	public static void Write( string message )
	{
		Log.Info( message );
	}


}
