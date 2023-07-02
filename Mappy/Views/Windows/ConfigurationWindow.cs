using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using KamiLib.Commands;
using KamiLib.Interfaces;
using KamiLib.Utilities;
using KamiLib.Windows;

namespace Mappy.Views.Windows;

public class ConfigurationWindow : TabbedSelectionWindow
{
    private readonly List<ISelectionWindowTab> tabs;
    private readonly List<ITabItem> regularTabs;

    public ConfigurationWindow() : base("Mappy - Configuration Window", 50.0f, 150.0f)
    {
        tabs = new List<ISelectionWindowTab>(Reflection.ActivateOfInterface<ISelectionWindowTab>());
        regularTabs = new List<ITabItem>(Reflection.ActivateOfInterface<ITabItem>());
        
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(560, 450),
            MaximumSize = new Vector2(9999,9999),
        };
        
        Flags |= ImGuiWindowFlags.NoScrollbar;
        Flags |= ImGuiWindowFlags.NoScrollWithMouse;
        
        CommandController.RegisterCommands(this);
    }
    
    protected override IEnumerable<ISelectionWindowTab> GetTabs() => tabs;
    protected override IEnumerable<ITabItem> GetRegularTabs() => regularTabs;
    
    public override bool DrawConditions()
    {
        if (Service.ClientState.IsPvP) return false;

        return true;
    }

    protected override void DrawWindowExtras()
    {
        PluginVersion.Instance.DrawVersionText();
        
        base.DrawWindowExtras();
    }
    
    [BaseCommandHandler("OpenConfigWindow")]
    public void OpenConfigWindow()
    {
        if (Service.ClientState.IsPvP) return;
            
        Toggle();
    }
}