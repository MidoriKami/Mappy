using ImGuiNET;
using KamiLib.Interfaces;
using KamiLib.Utilities;
using Mappy.Abstracts;

namespace Mappy.Views.Components;

public class ModuleSelectable : ISelectable, IDrawable
{
    public IDrawable Contents => this;
    public string ID => module.ModuleName.GetLabel();
    private readonly ModuleBase module;

    public ModuleSelectable(ModuleBase module) => this.module = module;

    public void DrawLabel() => ImGui.TextUnformatted(module.ModuleName.GetLabel());

    public void Draw() => module.DrawConfig();
}