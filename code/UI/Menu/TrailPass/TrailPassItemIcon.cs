using Sandbox.UI;

[UseTemplate]
internal class TrailPassItemIcon : Panel
{

	public TrailPassItem Item { get; set; }

	public TrailPassItemIcon( TrailPassItem item )
	{
		this.Item = item;

		var ticket = TrailPassTicket.Current;

		SetClass( "unlocked", ticket.Unlocked( item.Id ) );
	}

}
