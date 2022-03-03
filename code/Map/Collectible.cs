using Hammer;
using Sandbox;

[Library( "uf_collectible" )]
[EntityTool( "Unicycle Frenzy Collectible", "Unicycle Frenzy", "A prop that can be collected." )]
internal partial class Collectible : UfProp
{

	[Net, Property( "Collection", "Discretionary name of the collection group this collectible belongs to." )]
	public string Collection { get; set; }
	[Net, Property( "Solid" )]
	public new bool Solid { get; set; } = false;

}
