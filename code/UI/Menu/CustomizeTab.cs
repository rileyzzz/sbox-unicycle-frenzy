using Sandbox.UI;

[UseTemplate]
[NavigatorTarget( "menu/customize" )]
internal class CustomizeTab : Panel
{

	public CustomizeRenderScene RenderScene { get; set; }

	public CustomizeTab()
	{
		RebuildRenderScene();
	}

	public void RebuildRenderScene()
	{
		RenderScene?.BuildRenderScene();
	}

}
