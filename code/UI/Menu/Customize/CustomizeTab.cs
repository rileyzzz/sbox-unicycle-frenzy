using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using System.Linq;
using Facepunch.Customization;

[UseTemplate]
[NavigatorTarget("menu/customize")]
internal class CustomizeTab : Panel
{

    public CustomizeRenderScene RenderScene { get; set; }
    public Panel CategoryTabs { get; set; }
    public Panel PartsList { get; set; }

    public CustomizeTab()
    {
        UnicycleFrenzy.Game.CustomizationChanged += () =>
        {
            BuildRenderScene();
            BuildPartCategories();
        };
    }

    public override void OnHotloaded()
    {
        base.OnHotloaded();

        BuildRenderScene();
        BuildPartCategories();
    }

    protected override void PostTemplateApplied()
    {
        base.PostTemplateApplied();

        BuildRenderScene();
        BuildPartCategories();
    }

    public void BuildRenderScene()
    {
        RenderScene?.Build();
    }

    public void BuildParts(IEnumerable<CustomizationPart> parts)
    {
        PartsList.DeleteChildren(true);

        foreach (var part in parts)
        {
            if (!CanShowPart(part)) continue;

            var icon = new CustomizeItemButton(part);
            icon.Parent = PartsList;
        }
    }

    private void BuildPartCategories()
    {
        CategoryTabs.DeleteChildren( true );

        var categories = Customization.Config.Categories;
		var first = true;

        foreach ( var category in categories )
        {
			var btn = new CustomizeCategoryButton( category );
			btn.Parent = CategoryTabs;

			if ( first )
			{
				btn.SetActive();
				first = false;
			}
        }
    }

	private static bool CanShowPart( CustomizationPart part )
	{
		var cat = Customization.Config.Categories.FirstOrDefault( x => x.Id == part.CategoryId );

		if ( cat.DefaultPartId == part.Id ) return true;
		if ( TrailPassProgress.CurrentSeason.IsUnlockedByPartId( part.Id ) ) return true;

		return false;
	}

}
