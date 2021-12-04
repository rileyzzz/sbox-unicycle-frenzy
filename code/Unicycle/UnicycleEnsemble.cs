using System.Collections.Generic;
using System;
using System.Linq;

internal class UnicycleEnsemble
{

	public List<UnicyclePart> Parts = new();

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
