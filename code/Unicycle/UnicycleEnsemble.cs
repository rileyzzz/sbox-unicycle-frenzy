using System.Collections.Generic;
using System;
using System.Linq;
using Sandbox;

public partial class UnicycleEnsemble : BaseNetworkable
{

	public List<UnicyclePart> Parts = new();

	public UnicyclePart Wheel => Parts.FirstOrDefault( x => x.Type == PartType.Wheel );
	public UnicyclePart Seat => Parts.FirstOrDefault( x => x.Type == PartType.Seat );
	public UnicyclePart Frame => Parts.FirstOrDefault( x => x.Type == PartType.Frame );
	public UnicyclePart Pedal => Parts.FirstOrDefault( x => x.Type == PartType.Pedal );
	public UnicyclePart Trail => Parts.FirstOrDefault( x => x.Type == PartType.Trail );

	public void Equip( int hash ) => Equip( UnicyclePart.All.FirstOrDefault( x => x.GetHashCode() == hash ) );
	public void Unequip( int hash ) => Unequip( UnicyclePart.All.FirstOrDefault( x => x.GetHashCode() == hash ) );

	public void Equip( UnicyclePart part )
	{
		if ( part == null )
		{
			throw new Exception( "Equipping a null part" );
		}

		if ( Parts.Contains( part ) )
		{
			throw new Exception( "Equipping a part that is already equipped" );
		}

		var partInSlot = Parts.FirstOrDefault( x => x.Type == part.Type );
		if ( partInSlot != null )
		{
			Unequip( partInSlot );
		}

		Parts.Add( part );

		if ( Host.IsClient )
		{
			EquipPartOnServer( part.GetHashCode() );
		}
	}

	public void Unequip( UnicyclePart part )
	{
		if ( part == null )
		{
			throw new Exception( "Unequipping a null part" );
		}

		if ( !Parts.Contains( part ) )
		{
			throw new Exception( "Unequipping a part that isn't equipped" );
		}

		Parts.Remove( part );

		if ( Host.IsClient )
		{
			UnequipPartOnServer( part.GetHashCode() );
		}
	}

	[ServerCmd]
	public static void EquipPartOnServer( int hash )
	{
		var caller = ConsoleSystem.Caller;
		if ( caller == null ) return;

		var cfg = caller.Components.Get<ClientConfig>();
		if ( cfg == null ) return;

		cfg.Ensemble.Equip( hash );
	}

	[ServerCmd]
	public static void UnequipPartOnServer( int hash )
	{
		var caller = ConsoleSystem.Caller;
		if ( caller == null ) return;

		var cfg = caller.Components.Get<ClientConfig>();
		if ( cfg == null ) return;

		cfg.Ensemble.Unequip( hash );
	}

	public static UnicycleEnsemble Default
	{
		get
		{
			var result = new UnicycleEnsemble();
			foreach ( PartType partType in Enum.GetValues( typeof( PartType ) ) )
			{
				var part = UnicyclePart.All.FirstOrDefault( x => x.IsDefault && x.Type == partType );
				if ( part == null )
				{
					Log.Warning( "Missing default part type: " + partType );
					continue;
				}
				result.Parts.Add( part );
			}
			return result;
		}
	}

}
