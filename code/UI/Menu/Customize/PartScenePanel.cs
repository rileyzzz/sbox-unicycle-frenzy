using Facepunch.Customization;
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

	public PartScenePanel( CustomizationPart part, bool lookRight = false )
	{
		Build( part, lookRight );
	}

	private void Build( CustomizationPart part, bool lookRight )
	{
		using var _ = SceneWorld.SetCurrent( new SceneWorld() );

		Style.Width = Length.Percent( 100 );
		Style.Height = Length.Percent( 100 );

		var renderPanel = Add.ScenePanel( SceneWorld.Current, Vector3.Zero, Rotation.Identity, 35 );

		if ( part.AssetPath.EndsWith( "vpcf" ) )
		{
			var p = Particles.Create( part.AssetPath );
			p.SetPosition( 6, .75f );
			p.SetPosition( 7, 1 );
			p.SetPosition( 8, 0 );
			p.TimeScale = 100;

			renderPanel.CameraPosition = Vector3.Backward * 75 + Vector3.Down * 20;
			renderPanel.CameraRotation = Rotation.From( 0, 0, 0 );
			renderPanel.Style.Opacity = 1;
			renderPanel.RenderOnce = true;
		}
		else if ( part.AssetPath.EndsWith( "vmdl" ) )
		{
			var model = SceneObject.CreateModel( part.AssetPath, Transform.Zero );
			if ( lookRight ) model.Rotation = Rotation.LookAt( Vector3.Right );
			var bounds = model.Model.RenderBounds;

			renderPanel.CameraPosition = GetFocusPosition( bounds, Rotation.Identity, renderPanel.FieldOfView );
			renderPanel.CameraRotation = Rotation.From( 0, 0, 0 );
			renderPanel.RenderOnce = true;
		}

		Light.Point( Vector3.Up * 150.0f, 200.0f, Color.White * 100 );
		Light.Point( Vector3.Forward * 150.0f, 200.0f, Color.White * 100 );
		Light.Point( Vector3.Backward * 150.0f, 200.0f, Color.White * 100 );
		Light.Point( Vector3.Right * 150.0f, 200.0f, Color.White * 100 );
		Light.Point( Vector3.Left * 150.0f, 200.0f, Color.White * 100 );

		renderPanel.Style.Width = Length.Percent( 100 );
		renderPanel.Style.Height = Length.Percent( 100 );
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

}
