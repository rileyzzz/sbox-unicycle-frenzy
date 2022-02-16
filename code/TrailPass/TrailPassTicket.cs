using System.Collections.Generic;

internal class TrailPassTicket
{
	public int TrailPassId { get; set; }
	public int Experience { get; set; }
	public List<int> UnlockedItems { get; set; } = new();

	public bool Unlocked( int id ) => UnlockedItems.Contains( id );

	public static TrailPassTicket Current
	{
		get
		{
			return new()
			{
				TrailPassId = 1,
				Experience = 560,
				UnlockedItems = new()
				{
					1
				}
			};
		}
	}
}
