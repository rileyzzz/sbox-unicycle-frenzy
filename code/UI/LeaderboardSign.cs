using Sandbox;
using Sandbox.UI;

[UseTemplate]
internal class LeaderboardSign : WorldPanel
{

	private LeaderboardPost post;

	public LeaderboardSign( LeaderboardPost post )
	{
		this.post = post;

		Reposition();
	}

	public override void Tick()
	{
		base.Tick();

		if ( !post.IsValid() )
		{
			Delete();
			return;
		}

		Reposition();
	}

	private void Reposition()
	{
		var width = 2300;
		var height = 1450;
		MaxInteractionDistance = 5000f;
		PanelBounds = new Rect( -width * .5f, -height * .5f, width, height );
		Position = post.Position + post.Rotation.Forward * 4 + Vector3.Up * 82;
		Rotation = post.Rotation;
	}

}

