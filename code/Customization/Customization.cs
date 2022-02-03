using Sandbox;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Facepunch.Customization;

public static class Customization
{

	private static ulong crc;
	private const string filePath = "config/customization.json";

	public static bool Dirty => FileSystem.Mounted.GetCRC( filePath ).Result != crc;

	public static async Task<CustomizationConfig> LoadConfig()
	{
		if ( !FileSystem.Mounted.FileExists( filePath ) ) return new();

		var json = FileSystem.Mounted.ReadAllText( filePath );
		var config = JsonSerializer.Deserialize<CustomizationConfig>( json );
		crc = await FileSystem.Mounted.GetCRC( filePath );

		return config;
	}

}
