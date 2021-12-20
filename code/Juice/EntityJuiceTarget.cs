
using Sandbox;

public class EntityJuiceTarget : IJuiceTarget
{

	private Entity entity;

	public EntityJuiceTarget( Entity entity )
	{
		this.entity = entity;
	}

	public bool IsValid()
	{
		return entity.IsValid();
	}

	public void SetScale( float scale )
	{
		entity.LocalScale = scale;
	}

}

