using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Linq;

[UseTemplate]
[NavigatorTarget("menu/customize")]
internal class CustomizeTab : Panel
{

	public Panel RenderPanel { get; set; }

	private ScenePanel renderScene;
	private Angles renderSceneAngles = new( 25.0f, 0.0f, 0.0f );
	private float renderSceneDistance = 100;
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

	public void RebuildRenderScene()
	{
		BuildRenderScene();
	}

	private void BuildRenderScene()
	{
		renderScene?.Delete(true);

		using ( SceneWorld.SetCurrent( new SceneWorld() ) )
		{
			GenerateModel();

			var skycolor = Color.Orange;

			var sceneLight = Entity.All.FirstOrDefault( x => x is EnvironmentLightEntity ) as EnvironmentLightEntity;
			if ( sceneLight.IsValid() )
			{
				skycolor = sceneLight.SkyColor;
			}
			
			Light.Point( Vector3.Up * 150.0f, 200.0f, Color.White * 5.0f );
			Light.Point( Vector3.Up * 10.0f + Vector3.Forward * 100.0f, 200, Color.White * 15.0f );
			Light.Point( Vector3.Up * 10.0f + Vector3.Backward * 100.0f, 200, Color.White * 15f );
			Light.Point( Vector3.Up * 10.0f + Vector3.Left * 100.0f, 200, skycolor * 20.0f );
			Light.Point( Vector3.Up * 10.0f + Vector3.Right * 100.0f, 200, Color.White * 15.0f );
			Light.Point( Vector3.Up * 100.0f + Vector3.Up, 200, Color.White * 15.0f );

			renderScene = RenderPanel.Add.ScenePanel( SceneWorld.Current, renderScenePos, Rotation.From( renderSceneAngles ), 75 );
			renderScene.Style.Width = Length.Percent(100);
			renderScene.Style.Height = Length.Percent(100);
		}
	}

	private SceneObject GenerateModel()
	{
		var frame = SceneObject.CreateModel( "models/parts/frames/dev_frame", Transform.Zero );
		var wheel = SceneObject.CreateModel( "models/parts/wheels/dev_wheel", Transform.Zero );
		var seat = SceneObject.CreateModel( "models/parts/seats/dev_seat", Transform.Zero );
		
		var hub = wheel.Model.GetAttachment( "Hub" );

		frame.Position = Vector3.Up * hub.Value.Position.z;

		var seatAttachment = frame.Model.GetAttachment( "Seat" );

		seat.Position = seatAttachment.Value.Position + frame.Position;

		// todo: pedals

		frame.AddChild( "wheel", wheel );
		frame.AddChild( "seat", seat );

		return frame;
	}

}
