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

	public void LoadParts( PartType type )
	{
		PartsList.DeleteChildren( true );

		var parts = UnicyclePart.All.Where( x => x.Type == type );
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
