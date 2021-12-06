using Sandbox;
using Sandbox.UI;

[UseTemplate]
internal class UfChatboxEntry : Panel
{

	public Label Name { get; set; }
	public Label Message { get; set; }

	private TimeSince timeSinceCreation;

	public UfChatboxEntry( string name, string message )
	{
		timeSinceCreation = 0;
		Name.Text = name;
		Message.Text = message;
	}

	public override void Tick()
	{
		base.Tick();

		if ( timeSinceCreation < 8 ) return;

		AddClass( "faded" );
	}

}

