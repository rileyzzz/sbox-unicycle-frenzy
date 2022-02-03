using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using System.Linq;

using Facepunch.Customization;
using Sandbox;

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

		var cfg = Customization.Config;

		foreach( var category in cfg.Categories )
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

	private TimeSince timeSinceDirtyCheck;

	[Event.Frame]
	private async void OnFrame()
	{
		if ( timeSinceDirtyCheck < 1f ) return;
		timeSinceDirtyCheck = 0;

		//todo: FileSystem.Watcher so we can dodge this bs
		if ( await Customization.IsDirty() )
		{
			await Customization.LoadConfig();

			BuildRenderScene();
			BuildPartTypeButtons();
		}
	}

}
