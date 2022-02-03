using Sandbox;
using Sandbox.UI;
using System.IO;
using System.Linq;

[UseTemplate]
internal class CustomizePartIcon : Button
{

	public CustomizationPart Part { get; }

	public string PartName => Part.DisplayName;

	public CustomizePartIcon( CustomizationPart part )
	{
		Part = part;

		SetIcon();
	}

	protected override void OnClick( MousePanelEvent e )
	{
		base.OnClick( e );

		var ensemble = Local.Client.Components.Get<UnicycleEnsemble>();

		//ensemble.Equip( Part );

		Ancestors.OfType<CustomizeTab>().FirstOrDefault()?.BuildRenderScene();
	}

	public override void Tick()
	{
		base.Tick();

		var ensemble = Local.Client.Components.Get<UnicycleEnsemble>();
		//SetClass( "equipped", ensemble.Parts.Contains( Part ) );
	}

	private void SetIcon()
	{
		SetClass( "missing-icon", true );

		//if ( Part.Type == PartType.Spray )
		//{
		//	var texname = Path.GetFileNameWithoutExtension( Part.Model );
		//	var texpath = $"textures/sprays/{texname}.png";
		//	var tex = Texture.Load( FileSystem.Mounted, texpath, false );
		//	if ( tex == null ) return;
		//	Style.SetBackgroundImage( tex );
		//	SetClass( "missing-icon", false );
		//}
	}

}

