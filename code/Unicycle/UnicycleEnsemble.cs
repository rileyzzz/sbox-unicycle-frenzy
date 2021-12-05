using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

internal partial class UnicycleEnsemble : EntityComponent
{

	//[ConVar.ClientData( "unicycle", Saved = true )]
	public static string UnicycleJson
	{
		get => Cookie.Get( "unicycle", string.Empty );
		set => Cookie.Set( "unicycle", value );
	}

	public List<UnicyclePart> Parts = new();

	protected override void OnActivate()
	{
		base.OnActivate();

		if ( Entity.IsClient )
		{
			Deserialize( UnicycleJson );
		}
	}

	public UnicyclePart GetPart( PartType type )
	{
		return Parts.FirstOrDefault( x => x.Type == type ) ?? DefaultParts.FirstOrDefault( x => x.Type == type );
	}

	public void Load( Client cl )
	{
		Deserialize( cl.GetClientData( "unicycle", string.Empty ) );
	}

	public void Equip( int id ) => Equip( UnicyclePart.All.FirstOrDefault( x => x.Id == id ) );
	public void Equip( UnicyclePart part )
	{
		if ( part == null )
		{
			throw new Exception( "Equipping a null part" );
		}

		if ( Parts.Contains( part ) )
		{
			//throw new Exception( "Equipping a part that is already equipped" );
			return;
		}

		var partInSlot = Parts.FirstOrDefault( x => x.Type == part.Type );
		if ( partInSlot != null )
		{
			Unequip( partInSlot );
		}

		Parts.Add( part );

		if ( Host.IsClient )
		{
			UnicycleJson = Serialize();
			EquipPartOnServer( part.Id );
		}
	}

	public void Unequip( int id ) => Unequip( UnicyclePart.All.FirstOrDefault( x => x.Id == id ) );
	public void Unequip( UnicyclePart part )
	{
		if ( part == null )
		{
			throw new Exception( "Unequipping a null part" );
		}

		if ( !Parts.Contains( part ) )
		{
			//throw new Exception( "Unequipping a part that isn't equipped" );
			return;
		}

		Parts.Remove( part );

		if ( Host.IsClient )
		{
			UnicycleJson = Serialize();
			UnequipPartOnServer( part.Id );
		}
	}

	public string Serialize()
	{
		return System.Text.Json.JsonSerializer.Serialize( Parts.Select( x => new Entry { Id = x.Id } ) );
	}

	public void Deserialize( string json )
	{
		Parts.Clear();

		if ( string.IsNullOrWhiteSpace( json ) )
			return;

		try
		{
			var entries = System.Text.Json.JsonSerializer.Deserialize<Entry[]>( json );

			foreach ( var entry in entries )
			{
				var item = UnicyclePart.All.FirstOrDefault( x => x.Id == entry.Id );
				if ( item == null ) continue;
				Equip( item );
			}
		}
		catch ( System.Exception e )
		{
			Log.Warning( e, "Error deserailizing clothing" );
		}
	}

	public int GetPartsHash()
	{
		int hash = 0;
		foreach ( var part in Parts )
		{
			hash = HashCode.Combine( hash, part.Id );
		}
		return hash;
	}

	public struct Entry
	{
		[JsonPropertyName( "id" )]
		public int Id { get; set; }
	}

	[ServerCmd]
	public static void EquipPartOnServer( int id )
	{
		var caller = ConsoleSystem.Caller;
		if ( caller == null ) return;

		var cfg = caller.Components.Get<UnicycleEnsemble>();
		if ( cfg == null ) return;

		cfg.Equip( id );
	}

	[ServerCmd]
	public static void UnequipPartOnServer( int id )
	{
		var caller = ConsoleSystem.Caller;
		if ( caller == null ) return;

		var cfg = caller.Components.Get<UnicycleEnsemble>();
		if ( cfg == null ) return;

		cfg.Unequip( id );
	}

	private static List<UnicyclePart> defaultParts;
	public static IReadOnlyList<UnicyclePart> DefaultParts
	{
		get
		{
			if ( defaultParts == null )
			{
				defaultParts = new List<UnicyclePart>();
				foreach ( PartType partType in Enum.GetValues( typeof( PartType ) ) )
				{
					var part = UnicyclePart.All.FirstOrDefault( x => x.IsDefault && x.Type == partType );
					if ( part == null )
					{
						Log.Warning( "Missing default part type: " + partType );
						continue;
					}
					defaultParts.Add( part );
				}
			}
			return defaultParts;
		}
	}

}

