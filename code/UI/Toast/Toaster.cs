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
		Award
	}

	public override void Tick()
	{
		base.Tick();

		if ( Input.Pressed( InputButton.Slot1 ) )
		{
			Toast( "Test celebration", ToastTypes.Celebrate );
		}

		if ( Input.Pressed( InputButton.Slot2 ) )
		{
			Toast( "Test award", ToastTypes.Award );
		}
	}

	private static Toaster instance;
	public Toaster() => instance = this;
	public static void Toast( string message, ToastTypes type ) => new Toast( message, type ).Parent = instance;

}
