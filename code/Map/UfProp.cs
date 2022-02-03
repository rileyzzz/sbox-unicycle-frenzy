using Sandbox;
using System.Linq;
using Hammer;

[Hammer.Model]
[Hammer.SupportsSolid]
[Library("uf_prop")]
[EntityTool( "Unicycle Frenzy Prop", "Unicycle Frenzy", "A model or Mesh that can be set to pass the camera through it." )]
internal partial class UfProp : ModelEntity
{

	[Net, Property("No Camera Collide", "The Unicycle camera will maintain its position when touching this prop")]
	public bool NoCameraCollide { get; set; }
	[Net, Property( "Camera fade", "This prop will fade out when it's between the player and the camera" )]
	public bool CameraFade { get; set; }

	public bool BlockingView = false;

	public override void Spawn()
	{
		base.Spawn();

		SetupPhysicsFromModel( PhysicsMotionType.Static );
		EnableAllCollisions = true;
	}

	[Event.Frame]
	private void OnFrame()
	{
		if ( !CameraFade ) return;

		RenderColor = RenderColor.WithAlpha( RenderColor.a.LerpTo( BlockingView ? .4f : 1f, Time.Delta * 6f ) );
	}

}
