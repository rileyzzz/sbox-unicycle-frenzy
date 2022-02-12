using Sandbox;
using Sandbox.UI;

[UseTemplate]
internal class NewPlayerOverlay : Panel
{

	public bool Open
	{
		get => HasClass( "open" );
		set => SetClass( "open", value );
	}

	public Panel Warning { get; set; }

	public NewPlayerOverlay()
	{
		ShowOrNot();
	}

	private async void ShowOrNot()
	{
		// just wait til the tutorial map is up
		var pkg = await Package.Fetch( "facepunch.uf_tutorial", false );
		if ( pkg == null )
		{
			Open = false;
			return;
		}

		var show = Cookie.Get( "uf.hasplayed", false );

		if ( show )
		{
			Open = false;
			return;
		}

		Cookie.Set( "uf.hasplayed", true );
		Open = true;
	}

	public void Close()
	{
		Open = false;
	}

	public void LoadTutorial()
	{
		if( !Global.IsListenServer )
		{
			Warning.Style.Display = DisplayMode.Flex;
		}
		else
		{
			UnicycleFrenzy.Game.ChangeMap( "facepunch.uf_tutorial" );
		}
	}

}
