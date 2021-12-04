using System.Collections.Generic;
using System;
using System.Linq;

internal class UnicycleEnsemble
{

	public List<UnicyclePart> Parts = new();

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
					Log.Error( "Missing default part type: " + partType );
					continue;
				}
				result.Parts.Add( part );
			}
			return result;
		}
	}

}
