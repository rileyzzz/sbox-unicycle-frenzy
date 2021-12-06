using Sandbox;
using Sandbox.UI;
using System.Linq;

[UseTemplate]
internal class UnicyclePartIcon : Button
{

	public UnicyclePart Part { get; set; }

	public string PartName => Part.Name;

	public UnicyclePartIcon( UnicyclePart part )
	{
		Part = part;
	}

	protected override void OnClick( MousePanelEvent e )
	{
		base.OnClick( e );

		var ensemble = Local.Client.Components.Get<UnicycleEnsemble>();

		ensemble.Equip( Part );

		Ancestors.OfType<CustomizeTab>().FirstOrDefault()?.BuildRenderScene();
	}

	public override void Tick()
	{
		base.Tick();

		var ensemble = Local.Client.Components.Get<UnicycleEnsemble>();
		SetClass( "equipped", ensemble.Parts.Contains( Part ) );
	}

}

