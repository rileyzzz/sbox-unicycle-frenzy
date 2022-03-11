using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using System.Linq;

using Facepunch.Customization;
using Sandbox;

[UseTemplate]
[NavigatorTarget("menu/customize")]
internal class CustomizeTab : Panel
{

    public CustomizeRenderScene RenderScene { get; set; }
    public Panel PartsTypeList { get; set; }
    public Panel PartsList { get; set; }

    public CustomizeTab()
    {
        UnicycleFrenzy.Game.CustomizationChanged += () =>
        {
            BuildRenderScene();
            BuildPartTypeButtons();
        };
    }

    public override void OnHotloaded()
    {
        base.OnHotloaded();

        BuildRenderScene();
        BuildPartTypeButtons();
    }

    protected override void PostTemplateApplied()
    {
        base.PostTemplateApplied();

        BuildRenderScene();
        BuildPartTypeButtons();
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

            var icon = new CustomizePartIcon(part);
            icon.Parent = PartsList;
        }
    }

    private bool CanShowPart(CustomizationPart part)
    {
        var cat = Customization.Config.Categories.FirstOrDefault(x => x.Id == part.CategoryId);

        if (cat.DefaultPartId == part.Id) return true;
        if (TrailPassProgress.CurrentSeason.IsUnlockedByPartId(part.Id)) return true;

        return false;
    }

    private Button activeBtn;
    private void BuildPartTypeButtons()
    {
        PartsTypeList.DeleteChildren();
        activeBtn = null;

        var cfg = Customization.Config;

        foreach (var category in cfg.Categories)
        {
            var btn = PartsTypeList.Add.Button(category.DisplayName);
            btn.AddClass("tab");

            if (activeBtn == null)
            {
                activeBtn = btn;
                activeBtn.AddClass("active");

                BuildParts(cfg.Parts.Where(x => x.CategoryId == category.Id));
            }



            btn.AddEventListener("onclick", () =>
           {
               activeBtn?.RemoveClass("active");
               btn.AddClass("active");
               activeBtn = btn;

               BuildParts(cfg.Parts.Where(x => x.CategoryId == category.Id));
           });
        }

    }

}
