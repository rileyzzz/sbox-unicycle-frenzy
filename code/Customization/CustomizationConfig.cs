using Sandbox;
using System;
using System.Text.Json;
using System.Collections.Generic;

public class CustomizationConfig
{

	public static CustomizationConfig Gamemode
	{
		get
		{
			var filepath = "config/customization.json";

			if ( !FileSystem.Mounted.FileExists( filepath ) ) return new();

			var json = FileSystem.Mounted.ReadAllText( filepath );

			try
			{
				return JsonSerializer.Deserialize<CustomizationConfig>( json );
			}
			catch ( Exception e )
			{
				Log.Error( "Failed to deserialize Customization: " + e.Message );
			}

			return new();
		}
	}

	//public int CategoryIdAccumulator { get; set; }
	//public int PartIdAccumulator { get; set; }
	public List<CustomizationCategory> Categories { get; set; } = new();
	public List<CustomizationPart> Parts { get; set; } = new();

}

public class CustomizationCategory
{

	public int Id { get; set; }
	public string DisplayName { get; set; }
	public string IconPath { get; set; }
	public int DefaultPartId { get; set; }

}

public class CustomizationPart
{

	public int Id { get; set; }
	public int CategoryId { get; set; }
	public string DisplayName { get; set; }
	public string IconPath { get; set; }
	public string AssetPath { get; set; }

}
