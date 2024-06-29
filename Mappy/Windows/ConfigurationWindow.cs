using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using KamiLib.CommandManager;
using KamiLib.Components;
using KamiLib.Extensions;
using KamiLib.Window;
using Mappy.Data;

namespace Mappy.Windows;

public class ConfigurationWindow : Window {
    private readonly TabBar tabBar = new("mappy_tab_bar", [
        new IconConfigurationTab(),
        new WindowOptionsTab(),
        new FadeOptionsTab(),
        new PlayerOptionsTab(),
        new ToolbarOptionsTab(),
    ]);
    
    public ConfigurationWindow() : base("Mappy Configuration Window", new Vector2(400.0f, 300.0f)) {
        System.CommandManager.RegisterCommand(new CommandHandler {
            Delegate = _ => System.ConfigWindow.Toggle(),
            ActivationPath = "/",
        });
    }
    
    protected override void DrawContents() 
        => tabBar.Draw();
}

public class WindowOptionsTab : ITabItem {
    public string Name => "Map Window";
    public bool Disabled => false;
    public void Draw() {
        var configChanged = ImGui.Checkbox("Keep Open", ref System.SystemConfig.KeepOpen);
        configChanged |= ImGui.Checkbox("Ignore Escape Key", ref System.SystemConfig.IgnoreEscapeKey);
        configChanged |= ImGui.Checkbox("Follow On Open", ref System.SystemConfig.FollowOnOpen);        
        configChanged |= ImGui.Checkbox("Remember Last Map [Experimental]", ref System.SystemConfig.RememberLastMap);        
        
        ImGuiHelpers.ScaledDummy(5.0f);

        configChanged |= ImGui.Checkbox("Lock Window Position", ref System.SystemConfig.LockWindow);
        configChanged |= ImGui.Checkbox("Hide Window Frame", ref System.SystemConfig.HideWindowFrame);
        
        ImGuiHelpers.ScaledDummy(5.0f);
        
        configChanged |= ImGui.Checkbox("Hide With Game GUI", ref System.SystemConfig.HideWithGameGui);
        configChanged |= ImGui.Checkbox("Hide Between Areas", ref System.SystemConfig.HideBetweenAreas);
        configChanged |= ImGui.Checkbox("Hide in Duties", ref System.SystemConfig.HideInDuties);
        configChanged |= ImGui.Checkbox("Hide in Combat", ref System.SystemConfig.HideInCombat);
        
        ImGuiHelpers.ScaledDummy(5.0f);
        
        configChanged |= ImGui.Checkbox("Use Linear Zoom", ref System.SystemConfig.UseLinearZoom);
        configChanged |= ImGui.Checkbox("Show Misc Tooltips", ref System.SystemConfig.ShowMiscTooltips);

        ImGuiHelpers.ScaledDummy(5.0f);

        configChanged |= ImGui.DragFloat2("Window Position", ref System.SystemConfig.WindowPosition);
        configChanged |= ImGui.DragFloat2("Window Size", ref System.SystemConfig.WindowSize);
        configChanged |= ImGui.SliderFloat("Zoom Speed", ref System.SystemConfig.ZoomSpeed, 0.001f, 0.500f);
        configChanged |= ImGui.SliderFloat("Icon Scale", ref System.SystemConfig.IconScale, 0.10f, 3.0f);

        if (configChanged) {
            System.SystemConfig.Save();
        }
    }
}

public class FadeOptionsTab : ITabItem {
    public string Name => "Window Fade";
    public bool Disabled => false;
    public void Draw() {
        using (var columns = ImRaii.Table("fade_options_toggles", 2)) {
            if (!columns) return;

            var value = System.SystemConfig.FadeMode;
            ImGui.TableNextColumn();

            foreach (Enum enumValue in Enum.GetValues(value.GetType())) {
                var isFlagSet = value.HasFlag(enumValue);
                if (ImGuiComponents.ToggleButton(enumValue.ToString(), ref isFlagSet)) {
                    var sourceValue = Convert.ToInt32(value);
                    var targetValue = Convert.ToInt32(enumValue);

                    if (value.HasFlag(enumValue)) {
                        System.SystemConfig.FadeMode = (FadeMode) Enum.ToObject(value.GetType(), sourceValue & ~targetValue);
                    }
                    else {
                        System.SystemConfig.FadeMode = (FadeMode) Enum.ToObject(value.GetType(), sourceValue | targetValue);
                    }

                    System.SystemConfig.Save();
                }
                ImGui.SameLine();
                ImGui.TextUnformatted(enumValue.GetDescription());

                ImGui.TableNextColumn();
            } 
        }

        if (ImGui.DragFloat("Fade Opacity", ref System.SystemConfig.FadePercent, 0.01f, 0.05f, 1.0f)) {
            System.SystemConfig.Save();
        }
    }
}

public class PlayerOptionsTab : ITabItem {
    public string Name => "Player";
    public bool Disabled => false;
    public void Draw() {
        var configChanged = ImGui.Checkbox("Follow Player", ref System.SystemConfig.FollowPlayer);
        configChanged |= ImGui.Checkbox("Show Radar Radius", ref System.SystemConfig.ShowRadar);
        
        ImGuiHelpers.ScaledDummy(5.0f);
        
        DrawCenterModeRadio();

        if (configChanged) {
            System.SystemConfig.Save();
        }
    }

    private void DrawCenterModeRadio() {
        var enumObject = System.SystemConfig.CenterOnOpen;
        var firstLine = true;

        foreach (Enum enumValue in Enum.GetValues(enumObject.GetType()))
        {
            if (!firstLine) ImGui.SameLine();

            if (ImGui.RadioButton(enumValue.GetDescription(), enumValue.Equals(enumObject)))
            {
                System.SystemConfig.CenterOnOpen = (CenterTarget) enumValue;
                System.SystemConfig.Save();
            }

            firstLine = false;
        }
        
        ImGui.SameLine(); 
        ImGui.Text("\t\tCenter on Open");
    }
}

public class ToolbarOptionsTab : ITabItem {
    public string Name => "Toolbar";
    public bool Disabled => false;
    public void Draw() {
        var configChanged = ImGui.Checkbox("Always Show", ref System.SystemConfig.AlwaysShowToolbar);
        configChanged |= ImGui.Checkbox("Show On Hover", ref System.SystemConfig.ShowToolbarOnHover);
        
        if (configChanged) {
            System.SystemConfig.Save();
        }
    }
}

public class IconConfigurationTab : ITabItem {
    public string Name => "Icon Settings";
    public bool Disabled => false;

    private IconSetting? currentSetting;

    public void Draw() {
        using (var _ = ImRaii.Child("left_child", new Vector2(32.0f * ImGuiHelpers.GlobalScale + ImGui.GetStyle().ItemSpacing.X , ImGui.GetContentRegionAvail().Y))) {
            using var scrollbarStyle = ImRaii.PushStyle(ImGuiStyleVar.ScrollbarSize, 0.0f);
            using var selectionList = ImRaii.ListBox("iconSelection", ImGui.GetContentRegionAvail());
            
            foreach (var (iconId, settings) in System.IconConfig.IconSettingMap.OrderBy(pairData =>  pairData.Key)) {
                if (iconId is 0) continue;

                var texture = Service.TextureProvider.GetFromGameIcon(iconId).GetWrapOrEmpty();
                var cursorStart = ImGui.GetCursorScreenPos();
                if (ImGui.Selectable($"##iconSelect{iconId}", currentSetting == settings, ImGuiSelectableFlags.None, ImGuiHelpers.ScaledVector2(32.0f, 32.0f))) {
                    currentSetting = currentSetting == settings ? null : settings;
                }  
                
                ImGui.SetCursorScreenPos(cursorStart);
                ImGui.Image(texture.ImGuiHandle, texture.Size / 2.0f);
            }
        }
        
        ImGui.SameLine();
       
        using (var _ = ImRaii.Child("right_child", ImGui.GetContentRegionAvail(), false, ImGuiWindowFlags.NoScrollbar)) {
            if (currentSetting is null) {
                using var textColor = ImRaii.PushColor(ImGuiCol.Text, KnownColor.Orange.Vector());

                ImGui.SetCursorPosY(ImGui.GetContentRegionAvail().Y / 2.0f);
                ImGuiHelpers.CenteredText("Select an Icon to Edit Settings");
            }
            else {
                var texture = Service.TextureProvider.GetFromGameIcon(currentSetting.IconId).GetWrapOrEmpty();
                var smallestAxis = MathF.Min(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y);

                if (ImGui.GetContentRegionAvail().X > ImGui.GetContentRegionAvail().Y) {
                    var remainingSpace = ImGui.GetContentRegionAvail().X - smallestAxis;
                    ImGui.SetCursorPosX(remainingSpace / 2.0f);
                }
                
                ImGui.Image(texture.ImGuiHandle, new Vector2(smallestAxis, smallestAxis), Vector2.Zero, Vector2.One, new Vector4(1.0f, 1.0f, 1.0f, 0.20f));
                ImGui.SetCursorPos(Vector2.Zero);
                
                ImGuiHelpers.ScaledDummy(5.0f);

                var settingsChanged = ImGui.Checkbox("Hide Icon", ref currentSetting.Hide);
                settingsChanged |= ImGui.Checkbox("Allow Tooltip", ref currentSetting.AllowTooltip);
                settingsChanged |= ImGui.Checkbox("Allow Click Interaction", ref currentSetting.AllowClick);
                
                ImGuiHelpers.ScaledDummy(5.0f);

                settingsChanged |= ImGui.DragFloat("Icon Scale", ref currentSetting.Scale, 0.01f, 0.05f, 20.0f);

                if (settingsChanged) {
                    System.IconConfig.Save();
                }
            }
        }
    }
}