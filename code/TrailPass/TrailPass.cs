using System.Collections.Generic;

internal class TrailPass
{
	public int Id { get; set; }
	public string DisplayName { get; set; }
	public int ExperiencePerLevel { get; set; } = 10;
	public int MaxExperience { get; set; } = 1000;
	public List<TrailPassItem> Items { get; set; } = new();

	public static TrailPass Current
	{
		get
		{
			return new()
			{
				Id = 1,
				DisplayName = "Test TrailPass",
				Items = new()
				{
					new() { Id = 1,		RequiredExperience = 10,	DisplayName = "Item",		PartId = 45 },
					new() { Id = 2,		RequiredExperience = 40,	DisplayName = "Item 2",		PartId = 49 },
					new() { Id = 3,		RequiredExperience = 60,	DisplayName = "Item 3",		PartId = 55 },
					new() { Id = 4,		RequiredExperience = 100,	DisplayName = "Item 4",		PartId = 59 },
					new() { Id = 5,		RequiredExperience = 120,	DisplayName = "Item 5",		PartId = 68 },
					new() { Id = 6,		RequiredExperience = 140,	DisplayName = "Item 6",		PartId = 70 },
					new() { Id = 7,		RequiredExperience = 1000,	DisplayName = "Item 7",		PartId = 72 }
				}
			};
		}
	}
}

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

internal class TrailPassItem
{
	public int Id { get; set; }
	public int RequiredExperience { get; set; }
	public string DisplayName { get; set; }
	public int PartId { get; set; }
}
