﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Facepunch.Customization;

public class CustomizationComponent : EntityComponent
{

	public static string EnsembleJson
	{
		get => Cookie.Get( "customization.ensemble", string.Empty );
		set => Cookie.Set( "customization.ensemble", value );
	}

	public List<CustomizationPart> Parts = new();

	protected override void OnActivate()
	{
		base.OnActivate();

		if ( Entity.IsClient )
		{
			Deserialize( EnsembleJson );
		}
	}

	public CustomizationPart GetEquippedPart( string category )
	{
		var cfg = Customization.Config;

		var cetgory = cfg.Categories.FirstOrDefault( x => x.DisplayName.Equals( category, StringComparison.OrdinalIgnoreCase ) );
		if ( cetgory == null ) return null;

		var part = Parts.FirstOrDefault( x => x.CategoryId == cetgory.Id );

		return part ?? cfg.Parts.FirstOrDefault( x => x.Id == cetgory.DefaultPartId );
	}

	public void Equip( int id ) => Equip( Customization.Config.Parts.FirstOrDefault( x => x.Id == id ) );
	public void Equip( CustomizationPart part )
	{
		if ( part == null ) throw new Exception("Can't equip null");

		if ( Parts.Contains( part ) )
		{
			//throw new Exception( "Equipping a part that is already equipped" );
			return;
		}

		var partInSlot = Parts.FirstOrDefault( x => x.CategoryId == part.CategoryId );
		if ( partInSlot != null )
		{
			Unequip( partInSlot );
		}

		Parts.Add( part );

		if ( Host.IsClient )
		{
			EnsembleJson = Serialize();
			EquipPartOnServer( part.Id );
		}
	}

	public void Unequip( int id ) => Unequip( Customization.Config.Parts.FirstOrDefault( x => x.Id == id ) );
	public void Unequip( CustomizationPart part )
	{
		if ( part == null ) throw new Exception( "Can't equip null" );

		if ( !Parts.Contains( part ) )
		{
			//throw new Exception( "Unequipping a part that isn't equipped" );
			return;
		}

		Parts.Remove( part );

		if ( Host.IsClient )
		{
			EnsembleJson = Serialize();
			UnequipPartOnServer( part.Id );
		}
	}

	public bool IsEquipped( CustomizationPart part )
	{
		if ( part == null ) throw new Exception( "Can't equip null" );

		return Parts.Any( x => x.Id == part.Id );
	}

	public string Serialize()
	{
		return JsonSerializer.Serialize( Parts.Select( x => new Entry { Id = x.Id } ) );
	}

	public void Deserialize( string json )
	{
		Parts.Clear();

		if ( string.IsNullOrWhiteSpace( json ) )
			return;

		try
		{
			var entries = JsonSerializer.Deserialize<Entry[]>( json );

			foreach ( var entry in entries )
			{
				var item = Customization.Config.Parts.FirstOrDefault( x => x.Id == entry.Id );
				if ( item == null ) continue;
				Equip( item );
			}
		}
		catch ( Exception e )
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

		var cfg = caller.Components.Get<CustomizationComponent>();
		if ( cfg == null ) return;

		cfg.Equip( id );
	}

	[ServerCmd]
	public static void UnequipPartOnServer( int id )
	{
		var caller = ConsoleSystem.Caller;
		if ( caller == null ) return;

		var cfg = caller.Components.Get<CustomizationComponent>();
		if ( cfg == null ) return;

		cfg.Unequip( id );
	}

}

