using Sandbox;
using Sandbox.UI;

[UseTemplate]
internal class UfNametag : WorldPanel
{

	private UnicyclePlayer player;

	public string Name { get; set; }

	public UfNametag( UnicyclePlayer player )
	{
		this.player = player;

		if ( player.IsLocalPawn ) AddClass( "local" );
	}

	[Event.Frame]
	private void OnFrame()
	{
		if( !player.IsValid() )
		{
			Delete();
			return;
		}

		if ( !player.Client.IsValid() ) return;
		if ( !player.Terry.IsValid() ) return;

		var width = 500;
		var height = 500;
		PanelBounds = new Rect( -width * .5f, -height * .5f, width, height );

		var hat = player.Terry.GetAttachment( "hat" ) ?? new Transform( player.EyePos );
		
		Name = player.Client.Name;
		Position = hat.Position + Vector3.Up * 8;
		Rotation = Rotation.LookAt( (CurrentView.Position - player.Position).Normal );
		Style.Opacity = player.IsLocalPawn ? 0 : player.GetRenderAlpha();
	}

}
