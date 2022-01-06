using Sandbox;
using Sandbox.UI;

[UseTemplate]
internal class MapVoteButton : Button
{

	public Image Thumbnail { get; set; }
	public string Votes { get; } = "0 votes";
	public string MapIdent { get; }

	public MapVoteButton( string mapIdent )
	{
		MapIdent = mapIdent;

		SetThumbnail();
	}

	protected override void OnClick( MousePanelEvent e )
	{
		base.OnClick( e );


	}

	private async void SetThumbnail()
	{
		var pkg = await Package.Fetch( MapIdent, true );
		if ( pkg == null )
		{
			Delete( false );
			return;
		}

		Thumbnail.SetTexture( pkg.Thumb );
	}

}
