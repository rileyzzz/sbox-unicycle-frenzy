using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
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

		BuildAll();
	}

	protected override void PostTemplateApplied()
	{
		base.PostTemplateApplied();

		BuildAll();
	}

	public void BuildAll()
	{
		BuildRenderScene();
		BuildPartTypeButtons();
	}

	public void BuildRenderScene()
	{
		var ensemble = Local.Client.Components.Get<UnicycleEnsemble>();

		RenderScene?.Build( ensemble );
	}

	public void LoadParts( PartType type )
	{
		PartsList.DeleteChildren( true );

		var parts = UnicyclePart.All.Where( x => x.Type == type );
		foreach ( var part in parts )
		{
			var icon = new UnicyclePartIcon( part );
			icon.Parent = PartsList;
			icon.AddEventListener( "onclick", () => EquipPart( part ) );
		}
	}

	private void EquipPart( UnicyclePart part )
	{
		var ensemble = Local.Client.Components.Get<UnicycleEnsemble>();

		ensemble.Equip( part );

		BuildRenderScene();
	}

	private Button activeBtn;
	private void BuildPartTypeButtons()
	{
		PartsTypeList.DeleteChildren();
		activeBtn = null;

		foreach ( PartType type in Enum.GetValues( typeof( PartType ) ) )
		{
			var btn = PartsTypeList.Add.Button( type.ToString() );

			if ( activeBtn == null )
			{
				activeBtn = btn;
				activeBtn.AddClass( "active" );
				LoadParts( type );
			}

			btn.AddEventListener( "onclick", () =>
			 {
				 activeBtn?.RemoveClass( "active" );
				 btn.AddClass( "active" );
				 activeBtn = btn;
				 LoadParts( type );
			 } );
		}
	}

}
