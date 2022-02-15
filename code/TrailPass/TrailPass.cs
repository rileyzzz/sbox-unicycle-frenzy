using System.Collections.Generic;

internal class TrailPass
{
	public int Id { get; set; }
	public string DisplayName { get; set; }
	public int ExperiencePerLevel { get; set; } = 10;
	public int MaxExperience { get; set; } = 1000;
	public List<int> Items { get; set; } = new();

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
					1
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
	public int TrailPassId { get; set; }
	public string DisplayName { get; set; }
	public int PartId { get; set; }
}
