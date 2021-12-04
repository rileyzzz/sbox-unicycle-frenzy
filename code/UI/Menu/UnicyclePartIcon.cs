using Sandbox.UI;

[UseTemplate]
internal class UnicyclePartIcon : Button
{

	public UnicyclePart Part { get; set; }

	public string PartName => Part.Name;

	public UnicyclePartIcon( UnicyclePart part )
	{
		Part = part;
	}

}

