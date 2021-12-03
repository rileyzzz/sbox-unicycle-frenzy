using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

[UseTemplate]
[NavigatorTarget("menu/customize")]
internal class CustomizeTab : Panel
{

	public Panel RenderPanel { get; set; }

	readonly ScenePanel scene;

	Angles CamAngles = new( 25.0f, 0.0f, 0.0f );
	float CamDistance = 50;
	Vector3 CamPos => Vector3.Up * 22 + CamAngles.Direction * -CamDistance;

	public CustomizeTab()
	{
		Style.FlexWrap = Wrap.Wrap;
		Style.JustifyContent = Justify.Center;
		Style.AlignItems = Align.Center;
		Style.AlignContent = Align.Center;

		using ( SceneWorld.SetCurrent( new SceneWorld() ) )
		{
			SceneObject.CreateModel( "models/unicycle_dev.vmdl", Transform.Zero );

			Light.Point( Vector3.Up * 150.0f, 200.0f, Color.White * 5.0f );
			Light.Point( Vector3.Up * 10.0f + Vector3.Forward * 100.0f, 200, Color.White * 15.0f );
			Light.Point( Vector3.Up * 10.0f + Vector3.Backward * 100.0f, 200, Color.White * 15f );
			Light.Point( Vector3.Up * 10.0f + Vector3.Left * 100.0f, 200, Color.Orange * 15.0f );

			scene = RenderPanel.Add.ScenePanel( SceneWorld.Current, CamPos, Rotation.From( CamAngles ), 60 );
			scene.Style.Width = 512;
			scene.Style.Height = 512;
		}
	}

	public override void OnMouseWheel( float value )
	{
		CamDistance += value * 3;
		CamDistance = CamDistance.Clamp( 10, 200 );

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
			CamAngles.pitch += Mouse.Delta.y;
			CamAngles.yaw -= Mouse.Delta.x;
			CamAngles.pitch = CamAngles.pitch.Clamp( 0, 90 );
		}

		scene.CameraPosition = scene.CameraPosition.LerpTo( CamPos, 10f * Time.Delta );
		scene.CameraRotation = Rotation.Lerp( scene.CameraRotation, Rotation.From( CamAngles ), 10f * Time.Delta );
	}

}
