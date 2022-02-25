
using Sandbox;
using Sandbox.UI;

[UseTemplate]
internal class ControlHint : Panel
{

	public Label ActionName { get; set; }
	public Panel Icon { get; set; }

	private InputButton button;
	private Texture customTexture;

	public void Set( string name, InputButton button, bool hasCharge = false )
	{
		SetClass( "open", true );
		SetClass( "has-charge", hasCharge );

		ActionName.Text = name;
		customTexture = null;
		this.button = button;
	}

	public void Set( string name, TutorialTrigger.InputActions action, bool hasCharge = false )
	{
		SetClass( "open", true );
		SetClass( "has-charge", hasCharge );

		ActionName.Text = name;
		customTexture = GetCustomTexture( action );
		button = InputButton.Chat;
	}

	public override void Tick()
	{
		base.Tick();

		var tex = customTexture ?? Input.GetGlyph( button, InputGlyphSize.Large, GlyphStyle.Light );
		Icon.Style.BackgroundImage = tex;
	}

	private Texture GetCustomTexture( TutorialTrigger.InputActions action )
	{
		var path = action switch
		{
			TutorialTrigger.InputActions.Lean => "textures/ui/control_lean",
			TutorialTrigger.InputActions.Look => "textures/ui/control_look",
			_ => ""
		};

		if ( Input.UsingController )
			path += "_controller";

		path += ".png";

		return Texture.Load( FileSystem.Mounted, path );
	}

}
