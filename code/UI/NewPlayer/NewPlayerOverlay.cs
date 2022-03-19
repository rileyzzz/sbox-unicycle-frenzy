
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
		Open = ShouldShow();

		Cookie.Set( "uf.hasplayed", true );
	}

	private bool ShouldShow()
	{
		if ( Global.MapName.EndsWith( "uf_tutorial" ) ) return false;

		return !Cookie.Get( "uf.hasplayed", false );
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
