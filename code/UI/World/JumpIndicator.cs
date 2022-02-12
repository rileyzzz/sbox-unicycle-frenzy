using Sandbox;
using Sandbox.UI;

internal class JumpIndicator : WorldPanel
{

	private UnicyclePlayer pl;

	public JumpIndicator( UnicyclePlayer pl )
	{
		this.pl = pl;


	}

	public override void Tick()
	{
		base.Tick();

		if ( !pl.IsValid() ) return;
		if ( pl.Controller is not UnicycleController c ) return;

		var a = pl.TimeSinceJumpDown / c.MaxJumpStrengthTime;
		var sz = 100f.LerpTo( 500, a );
		var color = Color.Lerp( Color.White, Color.Green, a );
		var width = 5f.LerpTo( 10f, a );

		PanelBounds = new( sz * -.5f, sz * -.5f, sz, sz );
		Position = pl.Position;
		Rotation = Rotation.LookAt( Vector3.Up );

		Style.BorderBottomLeftRadius = Length.Percent( 50 );
		Style.BorderBottomRightRadius = Length.Percent( 50 );
		Style.BorderTopLeftRadius = Length.Percent( 50 );
		Style.BorderTopRightRadius = Length.Percent( 50 );
		Style.BorderColor = color;
		Style.BackgroundColor = Color.Transparent;
		Style.BorderWidth = Length.Pixels( width );
		Style.Opacity = a;
	}

}
