﻿using Sandbox;
using Sandbox.UI;
using System.IO;
using System.Linq;

using Facepunch.Customization;

[UseTemplate]
internal class CustomizePartIcon : Button
{

	public CustomizationPart Part { get; }

	public string PartName => Part.DisplayName;

	public CustomizePartIcon( CustomizationPart part )
	{
		Part = part;

		SetIcon();
		SetClass( "locked", !CanEquip() );
	}

	protected override void OnClick( MousePanelEvent e )
	{
		base.OnClick( e );

		if( !CanEquip() )
		{
			Toaster.Toast( "You haven't unlocked that yet", Toaster.ToastTypes.Error );
			return;
		}

		var customization = Local.Client.Components.Get<CustomizationComponent>();
		customization.Equip( Part );

		Ancestors.OfType<CustomizeTab>().FirstOrDefault()?.BuildRenderScene();
	}

	public override void Tick()
	{
		base.Tick();

		var customization = Local.Client.Components.Get<CustomizationComponent>();
		SetClass( "equipped", customization.IsEquipped( Part ) );
	}

	private bool CanEquip()
	{
		if ( TrailPassProgress.CurrentSeason.IsUnlockedByPartId( Part.Id ) ) 
			return true;

		var cat = Customization.Config.Categories.FirstOrDefault( x => x.Id == Part.CategoryId );
		if ( cat.DefaultPartId == Part.Id ) 
			return true;

		return false;
	}

	private void SetIcon()
	{
		var category = Customization.Config.Categories.First( x => x.Id == Part.CategoryId );

		switch ( category.DisplayName )
		{
			case "Wheel":
			case "Frame":
			case "Seat":
			case "Pedal":
			case "Trail":
				var lookright = category.DisplayName == "Wheel" || category.DisplayName == "Seat";
				new PartScenePanel( Part, lookright ).Parent = this;
				break;
			case "Spray":
				var texname = Path.GetFileNameWithoutExtension( Part.AssetPath );
				var texpath = $"textures/sprays/{texname}.png";
				var tex = Texture.Load( FileSystem.Mounted, texpath, false );
				if ( tex == null ) return;
				Style.SetBackgroundImage( tex );
				
				break;
			default:
				SetClass( "missing-icon", false );
				break;
		}

		var l = new Label();
		l.Parent = this;
		l.Text = Part.DisplayName;
	}

}

