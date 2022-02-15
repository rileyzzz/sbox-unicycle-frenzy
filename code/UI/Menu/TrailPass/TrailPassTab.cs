using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

[UseTemplate]
[NavigatorTarget( "menu/trailpass" )]
internal class TrailPassTab : Panel
{

	public Panel SceneCanvas { get; set; }
	public Panel ItemCanvas { get; set; }

	private SceneWorld sceneWorld;
	private ScenePanel renderScene;

	private void Rebuild()
	{
		BuildRenderScene();
		BuildItemList();
	}

	private void BuildItemList()
	{
		ItemCanvas.DeleteChildren();

		var trailpass = TrailPass.Current;

		foreach( var item in trailpass.Items )
		{
			var itemicon = new TrailPassItemIcon( item );
			itemicon.Parent = ItemCanvas;
		}
	}

	private void BuildRenderScene()
	{
		renderScene?.Delete();
		sceneWorld?.Delete();
		sceneWorld = new SceneWorld();

		using ( SceneWorld.SetCurrent( sceneWorld ) )
		{
			SceneObject.CreateModel( "models/room.vmdl", Transform.Zero.WithScale( 2 ).WithPosition( Vector3.Down * 4 ) );

			Light.Point( Vector3.Up * 150.0f, 200.0f, Color.White * 5.0f );
			Light.Point( Vector3.Up * 75.0f + Vector3.Forward * 100.0f, 200, Color.White * 15.0f );
			Light.Point( Vector3.Up * 75.0f + Vector3.Backward * 100.0f, 200, Color.White * 15f );
			Light.Point( Vector3.Up * 75.0f + Vector3.Left * 100.0f, 200, Color.White * 20.0f );

			renderScene = Add.ScenePanel( SceneWorld.Current, Vector3.Zero, Rotation.From( Angles.Zero ), 75 );
			renderScene.Style.Width = Length.Percent( 100 );
			renderScene.Style.Height = Length.Percent( 100 );
			renderScene.CameraPosition = new Vector3( -53, 100, 42 );
			renderScene.CameraRotation = Rotation.From( 12, 0, 0 );
			renderScene.Parent = SceneCanvas;
			//renderSceneAngles = renderScene.CameraRotation.Angles();

			var uicyce = SceneObject.CreateModel( "models/unicycle_dev.vmdl", Transform.Zero.WithScale( .1f ) );
			uicyce.Position = renderScene.CameraPosition + renderScene.CameraRotation.Forward * 15;
			uicyce.Position = uicyce.Position + renderScene.CameraRotation.Left;
			uicyce.Rotation = uicyce.Rotation.RotateAroundAxis( Vector3.Up, 90 );
			uicyce.Rotation = uicyce.Rotation.RotateAroundAxis( Vector3.Right, 10 );
		}
	}

	public override void OnHotloaded() => Rebuild();
	protected override void PostTemplateApplied() => Rebuild();

}
