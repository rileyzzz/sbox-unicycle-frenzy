
using Sandbox;

public class SceneObjectJuiceTarget : IJuiceTarget
{

	private SceneObject obj;

	public SceneObjectJuiceTarget( SceneObject obj )
	{
		this.obj = obj;
	}

	public bool IsValid()
	{
		return obj.IsValid();
	}

	public void SetScale( float scale )
	{
		obj.Transform = obj.Transform.WithScale( scale );
	}

	public void SetAlpha( float a )
	{
		obj.ColorTint = obj.ColorTint.WithAlpha( a );
	}

}

