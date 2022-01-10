using Sandbox;
using Sandbox.UI;
using System;

[UseTemplate]
internal class UfNametag : WorldPanel
{

	private UnicyclePlayer player;

	public string Name { get; set; }

	public UfNametag( UnicyclePlayer player )
	{
		this.player = player;

		if ( player.IsLocalPawn ) AddClass( "local" );

		var width = 1000;
		var height = 1000;
		PanelBounds = new Rect( -width * .5f, -height * .5f, width, height );
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

		var hat = player.Terry.GetAttachment( "hat" ) ?? new Transform( player.EyePos );
		Position = hat.Position + Vector3.Up * 8;
		Rotation = Rotation.LookAt( -Screen.GetDirection( new Vector2( Screen.Width * 0.5f, Screen.Height * 0.5f ) ) );
		Style.Opacity = player.IsLocalPawn ? 0 : player.GetRenderAlpha();

		var rank = player.SessionRank;
		var name = player.Client.Name;

		Name = player.CourseIncomplete ? name : $"#{rank} {name}";
	}

}
