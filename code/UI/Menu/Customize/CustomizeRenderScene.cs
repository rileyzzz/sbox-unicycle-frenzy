﻿using Facepunch.Customization;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Linq;

internal class CustomizeRenderScene : Panel
{

	private string prevtrail;
	private SceneObject unicycleObject;
	private Particles trailParticle;
	private SceneWorld sceneWorld;
	private ScenePanel renderScene;
	private Angles renderSceneAngles = new( 25.0f, 0.0f, 0.0f );
	private float renderSceneDistance = 135;
	private Vector3 renderScenePos => Vector3.Up * 22 + renderSceneAngles.Direction * -renderSceneDistance;

	private bool drag;

	public override void OnButtonEvent( ButtonEvent e )
	{
		if ( e.Button == "mouseleft" )
		{
			drag = e.Pressed;
		}

		base.OnButtonEvent( e );
	}

	[Event.Frame]
	private void OnFrame()
	{
		if ( renderScene == null ) return;

		if ( drag )
		{
			renderSceneAngles.pitch += Mouse.Delta.y * .5f;
			renderSceneAngles.yaw -= Mouse.Delta.x * .5f;
			renderSceneAngles.pitch = renderSceneAngles.pitch.Clamp( 0, 75 );
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

	private void BuildSceneWorld()
	{
		sceneWorld?.Delete();
		sceneWorld = new SceneWorld();

		using ( SceneWorld.SetCurrent( sceneWorld ) )
		{
			SceneObject.CreateModel( "models/scene/scene_unicycle_ensemble_main.vmdl", Transform.Zero.WithScale( 1 ).WithPosition( Vector3.Down * 4 ) );

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
			Light.Point( Vector3.Up * 100.0f + Vector3.Up, 200, Color.Yellow * 15.0f );

			renderScene = Add.ScenePanel( SceneWorld.Current, renderScenePos, Rotation.From( renderSceneAngles ), 75 );
			renderScene.Style.Width = Length.Percent( 100 );
			renderScene.Style.Height = Length.Percent( 100 );
			renderScene.CameraPosition = new Vector3( -53, 100, 42 );
			renderScene.CameraRotation = Rotation.From( 10, -62, 0 );
			renderSceneAngles = renderScene.CameraRotation.Angles();
		}
	}

	public void Build()
	{
		if ( sceneWorld == null ) BuildSceneWorld();
		using var _ = SceneWorld.SetCurrent( sceneWorld );

		unicycleObject?.Delete();

		var ensemble = Local.Client.Components.Get<CustomizationComponent>();
		unicycleObject = BuildUnicycleObject( ensemble );
	}

	private SceneObject BuildUnicycleObject( CustomizationComponent ensemble )
	{
		var frame = ensemble.GetEquippedPart( PartType.Frame.ToString() );
		var trail = ensemble.GetEquippedPart( PartType.Trail.ToString() );
		var wheel = ensemble.GetEquippedPart( PartType.Wheel.ToString() );
		var seat = ensemble.GetEquippedPart( PartType.Seat.ToString() );
		var pedal = ensemble.GetEquippedPart( PartType.Pedal.ToString() );

		var frameObj = SceneObject.CreateModel( frame.AssetPath, Transform.Zero );
		var wheelObj = SceneObject.CreateModel( wheel.AssetPath, Transform.Zero );
		var seatObj = SceneObject.CreateModel( seat.AssetPath, Transform.Zero );
		var pedalObjL = SceneObject.CreateModel( pedal.AssetPath, Transform.Zero );
		var pedalObjR = SceneObject.CreateModel( pedal.AssetPath, Transform.Zero );

		var frameHub = frameObj.Model.GetAttachment( "hub" ) ?? Transform.Zero;
		var wheelHub = wheelObj.Model.GetAttachment( "hub" ) ?? Transform.Zero;
		var wheelRadius = wheelHub.Position.z;

		frameObj.Position = Vector3.Up * (wheelRadius - frameHub.Position.z);

		var seatAttachment = frameObj.Model.GetAttachment( "seat" );

		seatObj.Position = seatAttachment.Value.Position + frameObj.Position;

		var pedalHub = pedalObjL.Model.GetAttachment( "hub" ) ?? Transform.Zero;

		pedalObjL.Position = (frameObj.Model.GetAttachment( "pedal_L" ) ?? Transform.Zero).Position;
		pedalObjL.Position += frameObj.Position - pedalHub.Position;

		pedalObjR.Position = (frameObj.Model.GetAttachment( "pedal_R" ) ?? Transform.Zero).Position;
		pedalObjR.Position += frameObj.Position + pedalHub.Position;
		pedalObjR.Rotation *= Rotation.From( 180, 180, 0 );
		

		frameObj.AddChild( "wheel", wheelObj );
		frameObj.AddChild( "seat", seatObj );
		frameObj.AddChild( "pedalL", pedalObjL );
		frameObj.AddChild( "pedalR", pedalObjR );

		if( prevtrail != trail.AssetPath )
		{
			prevtrail = trail.AssetPath;
			trailParticle?.Destroy( true );
			trailParticle = Particles.Create( trail.AssetPath, seatAttachment.Value.Position );
			trailParticle.SetPosition( 6, .75f );
			trailParticle.SetPosition( 7, 1 );
			trailParticle.SetPosition( 8, 0 );
		}

		Juice.Scale( 1, 1.15f, 1 )
			.WithDuration( .75f )
			.WithEasing( EasingType.BounceOut )
			.WithTarget( frameObj );

		return frameObj;
	}

}

