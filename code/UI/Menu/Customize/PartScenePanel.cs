﻿using Facepunch.Customization;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Linq;

internal class PartScenePanel : Panel
{

	//
	// todo: replace this with pngs, this is just a quick patch to hide missing part thumbs
	//

	public float RotationSpeed { get; set; }
	public bool RenderOnce
	{
		get => scenePanel.RenderOnce;
		set => scenePanel.RenderOnce = value;
	}

	private SceneObject sceneObj;
	private ScenePanel scenePanel;

	public PartScenePanel( CustomizationPart part, bool lookRight = false )
	{
		Build( part, lookRight );
	}

	private async void Build( CustomizationPart part, bool lookRight )
	{
		using var _ = SceneWorld.SetCurrent( new SceneWorld() );

		Style.Width = Length.Percent( 100 );
		Style.Height = Length.Percent( 100 );

		scenePanel = Add.ScenePanel( SceneWorld.Current, Vector3.Zero, Rotation.Identity, 35 );

		Particles p = null;
		if ( part.AssetPath.EndsWith( "vpcf" ) )
		{
			p = Particles.Create( part.AssetPath, Vector3.Zero );
			p.SetPosition( 6, .75f );
			p.SetPosition( 7, 1 );
			p.SetPosition( 8, 0 );
			p.TimeScale = 100;

			scenePanel.CameraPosition = Vector3.Backward * 75 + Vector3.Down * 20;
			scenePanel.CameraRotation = Rotation.From( 0, 0, 0 );
			scenePanel.Style.Opacity = 1;
			scenePanel.RenderOnce = true;
		}
		else if ( part.AssetPath.EndsWith( "vmdl" ) )
		{
			sceneObj = SceneObject.CreateModel( part.AssetPath, Transform.Zero );
			if ( lookRight ) sceneObj.Rotation = Rotation.LookAt( Vector3.Right );
			var bounds = sceneObj.Model.RenderBounds;

			scenePanel.CameraPosition = GetFocusPosition( bounds, Rotation.Identity, scenePanel.FieldOfView );
			scenePanel.CameraRotation = Rotation.From( 0, 0, 0 );
			scenePanel.RenderOnce = true;
		}

		Light.Point( Vector3.Up * 150.0f, 200.0f, Color.White * 100 );
		Light.Point( Vector3.Forward * 150.0f, 200.0f, Color.White * 100 );
		Light.Point( Vector3.Backward * 150.0f, 200.0f, Color.White * 100 );
		Light.Point( Vector3.Right * 150.0f, 200.0f, Color.White * 100 );
		Light.Point( Vector3.Left * 150.0f, 200.0f, Color.White * 100 );

		scenePanel.Style.Width = Length.Percent( 100 );
		scenePanel.Style.Height = Length.Percent( 100 );

		if( p != null )
		{
			await Task.Delay( 1500 );
			p.TimeScale = 1;
		}
	}

	private Vector3 GetFocusPosition( BBox bounds, Rotation cameraRot, float fov )
	{
		var focusDist = 0.75f;
		var maxSize = new[] { bounds.Size.x, bounds.Size.y, bounds.Size.z }.Max();
		var cameraView = 2.0f * (float)Math.Tan( 0.5f * 0.017453292f * fov );
		var distance = focusDist * maxSize / cameraView;
		distance += 0.5f * maxSize;
		return bounds.Center - distance * cameraRot.Forward;
	}

	[Event.Frame]
	private void OnFrame()
	{
		if ( RotationSpeed == 0 ) return;
		if ( !sceneObj.IsValid() ) return;
		if ( RenderOnce ) return;

		sceneObj.Rotation = sceneObj.Rotation.RotateAroundAxis( Vector3.Up, RotationSpeed * Time.Delta );
	}

}
