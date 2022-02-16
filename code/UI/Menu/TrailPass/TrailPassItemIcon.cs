using Facepunch.Customization;
using Sandbox.UI;
using System.Linq;

[UseTemplate]
internal class TrailPassItemIcon : Panel
{

	public TrailPassItem Item { get; set; }
	public Panel Thumbnail { get; set; }

	private PartScenePanel partPanel;

	public TrailPassItemIcon( TrailPassItem item )
	{
		this.Item = item;

		var ticket = TrailPassTicket.Current;

		SetClass( "unlocked", ticket.Unlocked( item.Id ) );

		var part = Customization.Config.Parts.FirstOrDefault( x => x.Id == item.PartId );
		if( part == null )
		{
			// show something else
			return;
		}

		var category = Customization.Config.Categories.FirstOrDefault( x => x.Id == part.CategoryId );
		var lookright = category.DisplayName == "Wheel" || category.DisplayName == "Seat";
		partPanel = new PartScenePanel( part, lookright );
		partPanel.RotationSpeed = 25f;

		Thumbnail.AddChild( partPanel );
	}

	protected override void OnMouseOver( MousePanelEvent e )
	{
		base.OnMouseOver( e );

		partPanel.RenderOnce = false;
	}

	protected override void OnMouseOut( MousePanelEvent e )
	{
		base.OnMouseOut( e );

		partPanel.RenderOnce = true;
	}

}
