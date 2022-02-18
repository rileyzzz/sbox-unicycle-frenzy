using Facepunch.Customization;
using Sandbox.UI;
using System;
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

		var progress = TrailPassProgress.CurrentSeason;

		SetClass( "unlocked", progress.IsUnlocked( item.Id ) );
		SetClass( "unlockable", progress.Experience >= item.RequiredExperience );

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

	public override void Tick()
	{
		base.Tick();

		var progress = TrailPassProgress.CurrentSeason;
		SetClass( "unlocked", progress.IsUnlocked( Item.Id ) );
		SetClass( "unlockable", Item.RequiredExperience <= progress.Experience );
	}

	public void TryUnlock()
	{
		var progress = TrailPassProgress.CurrentSeason;

		if ( progress.IsUnlocked( Item.Id ) )
		{
			Toaster.Toast( "You already unlocked that", Toaster.ToastTypes.Simple );
			return;
		}

		if ( Item.RequiredExperience > progress.Experience )
		{
			Toaster.Toast( $"You need {Item.RequiredExperience} xp!", Toaster.ToastTypes.Error );
			return;
		}

		progress.Unlock( Item.Id );
		progress.Save();

		Toaster.Toast( $"Item unlocked!", Toaster.ToastTypes.Celebrate );
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
