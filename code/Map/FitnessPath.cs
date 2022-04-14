using System;
using System.Linq;
using Sandbox;

[Library( "path_uf_fitness" )]
[Hammer.Path( "path_uf_fitness_node" )]
public partial class FitnessPath : BasePathEntity<FitnessNode>
{
	public static FitnessPath Current => Entity.All.OfType<FitnessPath>().FirstOrDefault();

	public FitnessPath()
	{
	}

	public FitnessNode GetNext(FitnessNode node)
	{
		int idx = PathNodes.IndexOf( node );
		if ( idx >= PathNodes.Count )
			return null;
		return PathNodes[idx + 1];
	}

	public FitnessNode GetPrev( FitnessNode node )
	{
		int idx = PathNodes.IndexOf( node );
		if ( idx <= 0 )
			return null;
		return PathNodes[idx - 1];
	}

	public static float GetSegmentDistance(Vector3 a, Vector3 b, Vector3 x)
	{
		Vector3 ab = b - a;
		Vector3 ax = x - a;


		if ( ax.Dot( ab ) <= 0.0f )
			return ax.Length;

		Vector3 bx = x - b;

		if ( bx.Dot( ab ) >= 0.0f )
			return bx.Length;

		return ab.Cross( ax ).Length / ab.Length;
	}

	public Vector3 GetNodePos(int i)
	{
		if ( i < 0 || i >= PathNodes.Count )
			return Vector3.Zero;

		return Transform.PointToWorld( PathNodes[i].Position );
	}

	public int GetClosestSegment(Vector3 pos)
	{
		float closestDist = float.MaxValue;
		int closest = -1;

		for ( int i = 0; i < PathNodes.Count - 1; i++ )
		{
			float dist = GetSegmentDistance( GetNodePos(i), GetNodePos(i + 1), pos );
			if (dist < closestDist)
			{
				closestDist = dist;
				closest = i;
			}
		}

		return closest;
	}

	public Vector3 GetPointAlongPath( float key )
	{
		int current = (int)MathF.Floor( key );
		int next = (int)MathF.Ceiling( key );

		if ( current < 0 )
		{
			current = 0;
			next = 1;
		}
		if ( next >= PathNodes.Count )
		{
			current = PathNodes.Count - 2;
			next = PathNodes.Count - 1;
		}
		return GetPointBetweenNodes( PathNodes[current], PathNodes[next], key - current );
	}

	public Vector3 GetDirectionAlongPath( float key )
	{
		int current = (int)MathF.Floor(key);
		int next = (int)MathF.Ceiling(key);

		if (current < 0)
		{
			current = 0;
			next = 1;
		}
		if (next >= PathNodes.Count)
		{
			current = PathNodes.Count - 2;
			next = PathNodes.Count - 1;
		}

		float dv = 0.01f;
		Vector3 a = GetPointBetweenNodes( PathNodes[current], PathNodes[next], key - current );
		Vector3 b = GetPointBetweenNodes( PathNodes[current], PathNodes[next], key - current + dv );

		//return (GetNodePos( next ) - GetNodePos( current )).Normal;
		return (b - a).Normal;
	}

	public Vector3 GetDirectionVector(Vector3 pos)
	{
		float key = GetKeyAlongPath( pos );
		Vector3 current = GetPointAlongPath( key );
		float dist = (pos - current).Length;
		Vector3 next = GetPointAlongPath( key + dist / 800.0f );

		return (next - current).Normal;
	}

	public float GetKeyAlongPath( Vector3 pos )
	{
		int closest = GetClosestSegment( pos );
		if ( closest < 0 || closest >= PathNodes.Count - 1)
			return 0.0f;

		Vector3 current = GetNodePos( closest );
		Vector3 next = GetNodePos( closest + 1 );

		float dist = (next - current).Length;
		Plane plane = new Plane(current, next - current);
		float key = closest + plane.GetDistance( pos ) / dist;
		return key;
	}
}
