using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;

[UseTemplate]
[NavigatorTarget( "menu/customize" )]
internal class CustomizeTab : Panel
{

	public CustomizeRenderScene RenderScene { get; set; }
	public Panel PartsTypeList { get; set; }
	public Panel PartsList { get; set; }

	public override void OnHotloaded()
	{
		base.OnHotloaded();

		BuildRenderScene();
		BuildPartTypeButtons();
	}

	protected override void PostTemplateApplied()
	{
		base.PostTemplateApplied();

		BuildRenderScene();
		BuildPartTypeButtons();
	}

	public void BuildRenderScene()
	{
		RenderScene?.Build();
	}

	public void BuildParts( IEnumerable<CustomizationPart> parts )
	{
		PartsList.DeleteChildren( true );

		foreach ( var part in parts )
		{
			var icon = new CustomizePartIcon( part );
			icon.Parent = PartsList;
		}
	}

	private Button activeBtn;
	private void BuildPartTypeButtons()
	{
		PartsTypeList.DeleteChildren();
		activeBtn = null;

		var cfg = CustomizationConfig.Gamemode;

		foreach( var category in CustomizationConfig.Gamemode.Categories )
		{
			var btn = PartsTypeList.Add.Button( category.DisplayName );

			if ( activeBtn == null )
			{
				activeBtn = btn;
				activeBtn.AddClass( "active" );
				BuildParts( cfg.Parts.Where( x => x.CategoryId == category.Id) );
			}

			btn.AddEventListener( "onclick", () =>
			{
				activeBtn?.RemoveClass( "active" );
				btn.AddClass( "active" );
				activeBtn = btn;
				BuildParts( cfg.Parts.Where( x => x.CategoryId == category.Id ) );
			} );
		}

	}

}
