using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Linq;

internal class CustomizeRenderScene : Panel
{

	private ScenePanel renderScene;
	private Angles renderSceneAngles = new( 25.0f, 0.0f, 0.0f );
	private float renderSceneDistance = 100;
	private Vector3 renderScenePos => Vector3.Up * 22 + renderSceneAngles.Direction * -renderSceneDistance;

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

		if ( renderScene == null ) return;

		if ( HasMouseCapture )
		{
			renderSceneAngles.pitch += Mouse.Delta.y;
			renderSceneAngles.yaw -= Mouse.Delta.x;
			renderSceneAngles.pitch = renderSceneAngles.pitch.Clamp( 0, 90 );
		}

		renderScene.CameraPosition = renderScene.CameraPosition.LerpTo( renderScenePos, 10f * Time.Delta );
		renderScene.CameraRotation = Rotation.Lerp( renderScene.CameraRotation, Rotation.From( renderSceneAngles ), 15f * Time.Delta );
	}

	public override void OnMouseWheel( float value )
	{
		renderSceneDistance += value * 3;
		renderSceneDistance = renderSceneDistance.Clamp( 10, 200 );

		base.OnMouseWheel( value );
	}

	public void Build( UnicycleEnsemble ensemble )
	{
		renderScene?.Delete( true );

		using ( SceneWorld.SetCurrent( new SceneWorld() ) )
		{
			GenerateModel( ensemble );

			SceneObject.CreateModel( "models/room.vmdl", Transform.Zero.WithScale( 10 ).WithPosition( Vector3.Down * 10 ) );

			var skycolor = Color.Orange;

			var sceneLight = Entity.All.FirstOrDefault( x => x is EnvironmentLightEntity ) as EnvironmentLightEntity;
			if ( sceneLight.IsValid() )
			{
				skycolor = sceneLight.SkyColor;
			}

			Light.Point( Vector3.Up * 150.0f, 200.0f, Color.White * 5.0f );
			Light.Point( Vector3.Up * 75.0f + Vector3.Forward * 100.0f, 200, Color.White * 15.0f );
			Light.Point( Vector3.Up * 75.0f + Vector3.Backward * 100.0f, 200, Color.White * 15f );
			Light.Point( Vector3.Up * 75.0f + Vector3.Left * 100.0f, 200, skycolor * 20.0f );
			Light.Point( Vector3.Up * 75.0f + Vector3.Right * 100.0f, 200, Color.White * 15.0f );
			Light.Point( Vector3.Up * 100.0f + Vector3.Up, 200, Color.White * 15.0f );

			renderScene = Add.ScenePanel( SceneWorld.Current, renderScenePos, Rotation.From( renderSceneAngles ), 75 );
			renderScene.Style.Width = Length.Percent( 100 );
			renderScene.Style.Height = Length.Percent( 100 );
			renderScene.CameraPosition = new Vector3( -53, 100, 42 );
			renderScene.CameraRotation = Rotation.From( 10, -62, 0 );
			renderSceneAngles = renderScene.CameraRotation.Angles();
		}
	}

	private SceneObject GenerateModel( UnicycleEnsemble ensemble )
	{
		var def = UnicycleEnsemble.Default;

		var frame = ensemble.Frame ?? def.Frame;
		var wheel = ensemble.Wheel ?? def.Wheel;
		var seat = ensemble.Seat ?? def.Seat;
		var pedal = ensemble.Pedal ?? def.Pedal;

		var frameObj = SceneObject.CreateModel( frame.Model, Transform.Zero );
		var wheelObj = SceneObject.CreateModel( wheel.Model, Transform.Zero );
		var seatObj = SceneObject.CreateModel( seat.Model, Transform.Zero );

		var hub = wheelObj.Model.GetAttachment( "Hub" );

		frameObj.Position = Vector3.Up * hub.Value.Position.z;

		var seatAttachment = frameObj.Model.GetAttachment( "Seat" );

		seatObj.Position = seatAttachment.Value.Position + frameObj.Position;

		// todo: pedals

		frameObj.AddChild( "wheel", wheelObj );
		frameObj.AddChild( "seat", seatObj );

		return frameObj;
	}

}

