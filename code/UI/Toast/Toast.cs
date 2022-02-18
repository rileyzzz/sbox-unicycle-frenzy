using Sandbox;
using Sandbox.UI;

[UseTemplate]
internal class Toast : Panel
{

	private TimeSince timeSinceCreated = 0;

	public Button Message { get; set; }

	public Toast( string message, Toaster.ToastTypes type )
	{
		AddClass( type.ToString() );
		Message.Text = message;
		Message.Icon = type switch
		{
			Toaster.ToastTypes.Celebrate => "celebration",
			Toaster.ToastTypes.Affirm => "check",
			Toaster.ToastTypes.Award => "star",
			_ => "info"
		};
	}

	public override void Tick()
	{
		base.Tick();

		if ( timeSinceCreated < 8 ) return;

		Delete();
	}

}
