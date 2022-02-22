using Sandbox;
using Sandbox.UI;

[UseTemplate]
internal class Toaster : Panel
{

	public enum ToastTypes
	{
		Simple,
		Celebrate,
		Affirm,
		Award,
		Error
	}

	private static Toaster instance;
	public Toaster() => instance = this;
	public static void Toast( string message, ToastTypes type ) => new Toast( message, type ).Parent = instance;

}
