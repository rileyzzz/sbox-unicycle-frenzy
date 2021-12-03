using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

[UseTemplate]
[NavigatorTarget("menu/customize")]
internal class CustomizeTab : Panel
{

	public Panel RenderPanel { get; set; }

	private ScenePanel renderScene;
	private Angles renderSceneAngles = new( 25.0f, 0.0f, 0.0f );
	private float renderSceneDistance = 50;
	private Vector3 renderScenePos => Vector3.Up * 22 + renderSceneAngles.Direction * -renderSceneDistance;

	public CustomizeTab()
	{
		BuildRenderScene();
	}

	public override void OnMouseWheel( float value )
	{
		renderSceneDistance += value * 3;
		renderSceneDistance = renderSceneDistance.Clamp( 10, 200 );

		base.OnMouseWheel( value );
	}

	public override void OnButtonEvent( ButtonEvent e )
	{
		if ( e.Button == "mouseleft" )
		{
			SetMouseCapture( e.Pressed );
		}

		base.OnButtonEvent( e );
	}

	public override void Tick()
	{
		base.Tick();

		if ( HasMouseCapture )
		{
			renderSceneAngles.pitch += Mouse.Delta.y;
			renderSceneAngles.yaw -= Mouse.Delta.x;
			renderSceneAngles.pitch = renderSceneAngles.pitch.Clamp( 0, 90 );
		}

		renderScene.CameraPosition = renderScene.CameraPosition.LerpTo( renderScenePos, 10f * Time.Delta );
		renderScene.CameraRotation = Rotation.Lerp( renderScene.CameraRotation, Rotation.From( renderSceneAngles ), 15f * Time.Delta );
	}

	private void BuildRenderScene()
	{
		using ( SceneWorld.SetCurrent( new SceneWorld() ) )
		{
			SceneObject.CreateModel( "models/unicycle_dev.vmdl", Transform.Zero );

			Light.Point( Vector3.Up * 150.0f, 200.0f, Color.White * 5.0f );
			Light.Point( Vector3.Up * 10.0f + Vector3.Forward * 100.0f, 200, Color.White * 15.0f );
			Light.Point( Vector3.Up * 10.0f + Vector3.Backward * 100.0f, 200, Color.White * 15f );
			Light.Point( Vector3.Up * 10.0f + Vector3.Left * 100.0f, 200, Color.Orange * 15.0f );

			renderScene = RenderPanel.Add.ScenePanel( SceneWorld.Current, renderScenePos, Rotation.From( renderSceneAngles ), 60 );
			renderScene.Style.Width = 512;
			renderScene.Style.Height = 512;
		}
	}

}
