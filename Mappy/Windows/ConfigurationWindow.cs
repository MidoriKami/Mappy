using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using KamiLib.Classes;
using KamiLib.CommandManager;
using KamiLib.Extensions;
using KamiLib.Window;
using Mappy.Classes;
using Mappy.Data;

namespace Mappy.Windows;

public class ConfigurationWindow : Window {
    private readonly TabBar tabBar = new("mappy_tab_bar", [
        new IconConfigurationTab(),
        new MapFunctionsTab(),
        new StyleOptionsTab(),
        new PlayerOptionsTab(),
    ]);
    
    public ConfigurationWindow() : base("Mappy Configuration Window", new Vector2(500.0f, 580.0f)) {
        System.CommandManager.RegisterCommand(new CommandHandler {
            Delegate = _ => System.ConfigWindow.Toggle(),
            ActivationPath = "/",
        });
    }
    
    protected override void DrawContents() 
        => tabBar.Draw();
}

public class MapFunctionsTab : ITabItem {
    public string Name => "Map Functions";
    
    public bool Disabled => false;
    
    public void Draw() {
        var configChanged = false;
        
        ImGuiTweaks.Header("Key Input");
        using (ImRaii.PushIndent()) {
            configChanged |= ImGui.Checkbox("Ignore Escape Key", ref System.SystemConfig.IgnoreEscapeKey);
        }
        
        ImGuiTweaks.Header("Zoom Options");
        using (ImRaii.PushIndent()) {
            configChanged |= ImGui.Checkbox("Use Linear Zoom", ref System.SystemConfig.UseLinearZoom);
            configChanged |= ImGui.Checkbox("Scale icons with zoom", ref System.SystemConfig.ScaleWithZoom);
            
            ImGuiHelpers.ScaledDummy(5.0f);
            
            configChanged |= ImGui.SliderFloat("Zoom Speed", ref System.SystemConfig.ZoomSpeed, 0.001f, 0.500f);
            configChanged |= ImGui.SliderFloat("Icon Scale", ref System.SystemConfig.IconScale, 0.10f, 3.0f);
        }
        
        ImGuiTweaks.Header("When Opening Map");
        using (ImRaii.PushIndent()) {
            configChanged |= ImGui.Checkbox("Follow On Open", ref System.SystemConfig.FollowOnOpen);   
            
            ImGuiHelpers.ScaledDummy(5.0f);

            DrawCenterModeRadio();
        }
        
        ImGuiTweaks.Header("Link Behaviors");
        using (ImRaii.PushIndent()) {
            configChanged |= ImGui.Checkbox("Center on Flags", ref System.SystemConfig.CenterOnFlag);
            configChanged |= ImGui.Checkbox("Center on Gathering Areas", ref System.SystemConfig.CenterOnGathering);
            configChanged |= ImGui.Checkbox("Center on Quest", ref System.SystemConfig.CenterOnQuest);
        }
        
        ImGuiTweaks.Header("Misc Options");
        using (ImRaii.PushIndent()) {
            configChanged |= ImGui.Checkbox("Show Misc Tooltips", ref System.SystemConfig.ShowMiscTooltips);
            configChanged |= ImGui.Checkbox("Lock Map on Center", ref System.SystemConfig.LockCenterOnMap);
            configChanged |= ImGui.Checkbox("Show Other Players", ref System.SystemConfig.ShowPlayers);
            
            ImGuiHelpers.ScaledDummy(5.0f);
            
            configChanged |= ImGui.Checkbox("Debug Mode", ref System.SystemConfig.DebugMode);
        }
        
        ImGuiTweaks.Header("Toolbar");
        using (ImRaii.PushIndent()) {
            configChanged |= ImGui.Checkbox("Always Show", ref System.SystemConfig.AlwaysShowToolbar);
            configChanged |= ImGui.Checkbox("Show On Hover", ref System.SystemConfig.ShowToolbarOnHover);
            
            ImGuiHelpers.ScaledDummy(5.0f);
            configChanged |= ImGui.DragFloat("Opacity##toolbar", ref System.SystemConfig.ToolbarFade, 0.01f, 0.0f, 1.0f);
        }
        
        ImGuiTweaks.Header("Coordinates");
        using (ImRaii.PushIndent()) {
            configChanged |= ImGui.Checkbox("Show Coordinate Bar", ref System.SystemConfig.ShowCoordinateBar);
            
            ImGuiHelpers.ScaledDummy(5.0f);
            configChanged |= ImGuiTweaks.ColorEditWithDefault("Text Color", ref System.SystemConfig.CoordinateTextColor, KnownColor.White.Vector());

            ImGuiHelpers.ScaledDummy(5.0f);
            configChanged |= ImGui.DragFloat("Opacity##coordinatebar", ref System.SystemConfig.CoordinateBarFade, 0.01f, 0.0f, 1.0f);
        }
        
        if (configChanged) {
            System.SystemConfig.Save();
        }
    }
    
    private void DrawCenterModeRadio() {
        var enumObject = System.SystemConfig.CenterOnOpen;
        var firstLine = true;

        foreach (Enum enumValue in Enum.GetValues(enumObject.GetType())) {
            if (!firstLine) ImGui.SameLine();

            if (ImGui.RadioButton(enumValue.GetDescription(), enumValue.Equals(enumObject))) {
                System.SystemConfig.CenterOnOpen = (CenterTarget) enumValue;
                System.SystemConfig.Save();
            }

            firstLine = false;
        }
        
        ImGui.SameLine(); 
        ImGui.Text("\t\tCenter on Open");
    }
}

public class StyleOptionsTab : ITabItem {
    public string Name => "Style";
    
    public bool Disabled => false;
    
    public void Draw() {
        var configChanged = false;
        
        ImGuiTweaks.Header("Window Options");
        using (ImRaii.PushIndent()) {
            configChanged |= ImGui.Checkbox("Keep Open", ref System.SystemConfig.KeepOpen);
            configChanged |= ImGui.Checkbox("Lock Window Position", ref System.SystemConfig.LockWindow);
            configChanged |= ImGui.Checkbox("Hide Window Frame", ref System.SystemConfig.HideWindowFrame);
        }
        
        ImGuiTweaks.Header("Window Hiding");
        using (ImRaii.PushIndent()) {
            configChanged |= ImGui.Checkbox("Hide With Game GUI", ref System.SystemConfig.HideWithGameGui);
            configChanged |= ImGui.Checkbox("Hide Between Areas", ref System.SystemConfig.HideBetweenAreas);
            configChanged |= ImGui.Checkbox("Hide in Duties", ref System.SystemConfig.HideInDuties);
            configChanged |= ImGui.Checkbox("Hide in Combat", ref System.SystemConfig.HideInCombat);
        }
        
        ImGuiTweaks.Header("Window Location");
        using (ImRaii.PushIndent()) {
            configChanged |= ImGui.DragFloat2("Window Position", ref System.SystemConfig.WindowPosition);
            configChanged |= ImGui.DragFloat2("Window Size", ref System.SystemConfig.WindowSize);
        }
        
        ImGuiTweaks.Header("Fade Options");
        using (ImRaii.PushIndent()) {
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

                        configChanged = true;
                    }
                    ImGui.SameLine();
                    ImGui.TextUnformatted(enumValue.GetDescription());

                    ImGui.TableNextColumn();
                } 
            }

            configChanged |= ImGui.DragFloat("Fade Opacity", ref System.SystemConfig.FadePercent, 0.01f, 0.05f, 1.0f);
        }
        
        ImGuiTweaks.Header("Area Style");
        using (ImRaii.PushIndent()) {
            configChanged |= ImGuiTweaks.ColorEditWithDefault("Area Color", ref System.SystemConfig.AreaColor, KnownColor.CornflowerBlue.Vector() with { W = 0.33f });
            configChanged |= ImGuiTweaks.ColorEditWithDefault("Area Outline Color", ref System.SystemConfig.AreaOutlineColor, KnownColor.CornflowerBlue.Vector() with { W = 0.30f });
        }

        if (configChanged) {
            if (System.MapWindow.SizeConstraints is { } constraints) {
                System.SystemConfig.WindowSize.X = MathF.Max(System.SystemConfig.WindowSize.X, constraints.MinimumSize.X);
                System.SystemConfig.WindowSize.Y = MathF.Max(System.SystemConfig.WindowSize.Y, constraints.MinimumSize.Y);
            }
            
            System.SystemConfig.Save();
        }
    }
}

public class PlayerOptionsTab : ITabItem {
    public string Name => "Player";
    
    public bool Disabled => false;
    
    public void Draw() {
        var configChanged = false;
        
        ImGuiTweaks.Header("Cone Options");
        using (ImRaii.PushIndent()) {
            configChanged |= ImGui.Checkbox("Scale Player Cone", ref System.SystemConfig.ScalePlayerCone);

            ImGuiHelpers.ScaledDummy(5.0f);
            configChanged |= ImGui.DragFloat("Cone Size", ref System.SystemConfig.ConeSize, 0.25f);

            ImGuiHelpers.ScaledDummy(5.0f);
            configChanged |= ImGuiTweaks.ColorEditWithDefault("Cone Color", ref System.SystemConfig.PlayerConeColor, KnownColor.CornflowerBlue.Vector() with { W = 0.33f });
            configChanged |= ImGuiTweaks.ColorEditWithDefault("Cone Outline Color", ref System.SystemConfig.PlayerConeOutlineColor, KnownColor.CornflowerBlue.Vector() with { W = 1.00f });
        }
        
        ImGuiTweaks.Header("Radar Options");
        using (ImRaii.PushIndent()) {
            configChanged |= ImGui.Checkbox("Show Radar Radius", ref System.SystemConfig.ShowRadar);

            ImGuiHelpers.ScaledDummy(5.0f);
            
            configChanged |= ImGuiTweaks.ColorEditWithDefault("Radar Area Color", ref System.SystemConfig.RadarColor, KnownColor.Gray.Vector() with { W = 0.10f });
            configChanged |= ImGuiTweaks.ColorEditWithDefault("Radar Outline Color", ref System.SystemConfig.RadarOutlineColor, KnownColor.Gray.Vector() with { W = 0.30f });
        }
        
        ImGuiTweaks.Header("Player Icon Options");
        using (ImRaii.PushIndent()) {
            configChanged |= ImGui.Checkbox("Show Player Icon", ref System.SystemConfig.ShowPlayerIcon);
            
            ImGuiHelpers.ScaledDummy(5.0f);
            configChanged |= ImGui.DragFloat("Player Icon Size", ref System.SystemConfig.PlayerIconScale, 0.05f);
        }

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
        using (var leftChild = ImRaii.Child("left_child", new Vector2(48.0f * ImGuiHelpers.GlobalScale + ImGui.GetStyle().ItemSpacing.X , ImGui.GetContentRegionAvail().Y))) {
            if (leftChild) {
                using var selectionList = ImRaii.ListBox("iconSelection", ImGui.GetContentRegionAvail());
            
                foreach (var (iconId, settings) in System.IconConfig.IconSettingMap.OrderBy(pairData =>  pairData.Key)) {
                    if (iconId is 0) continue;
                    if (DrawHelpers.IsDisallowedIcon(iconId)) continue;

                    var texture = Service.TextureProvider.GetFromGameIcon(iconId).GetWrapOrEmpty();
                    var cursorStart = ImGui.GetCursorScreenPos();
                    if (ImGui.Selectable($"##iconSelect{iconId}", currentSetting == settings, ImGuiSelectableFlags.None, ImGuiHelpers.ScaledVector2(32.0f, 32.0f))) {
                        currentSetting = currentSetting == settings ? null : settings;
                    }  
                
                    ImGui.SetCursorScreenPos(cursorStart);
                    ImGui.Image(texture.ImGuiHandle, texture.Size / 2.0f * ImGuiHelpers.GlobalScale);
                } 
            }
        }
        
        ImGui.SameLine();
       
        using (var rightChild = ImRaii.Child("right_child", ImGui.GetContentRegionAvail(), false, ImGuiWindowFlags.NoScrollbar)) {
            if (rightChild) {
                if (currentSetting is null) {
                    using var textColor = ImRaii.PushColor(ImGuiCol.Text, KnownColor.Orange.Vector());

                    ImGui.SetCursorPosY(ImGui.GetContentRegionAvail().Y / 2.0f);
                    ImGuiHelpers.CenteredText("Select an Icon to Edit Settings");
                }
                else {
                    // Draw background texture
                    var settingsChanged = false;
                    var texture = Service.TextureProvider.GetFromGameIcon(currentSetting.IconId).GetWrapOrEmpty();
                    var smallestAxis = MathF.Min(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y);

                    if (ImGui.GetContentRegionAvail().X > ImGui.GetContentRegionAvail().Y) {
                        var remainingSpace = ImGui.GetContentRegionAvail().X - smallestAxis;
                        ImGui.SetCursorPosX(remainingSpace / 2.0f);
                    }
                
                    ImGui.Image(texture.ImGuiHandle, new Vector2(smallestAxis, smallestAxis), Vector2.Zero, Vector2.One, new Vector4(1.0f, 1.0f, 1.0f, 0.20f));
                    ImGui.SetCursorPos(Vector2.Zero);
                    
                    // Draw settings
                    ImGuiTweaks.Header($"Configure Marker #{currentSetting.IconId}");
                    using (ImRaii.PushIndent()) {
                        settingsChanged |= ImGui.Checkbox("Hide Icon", ref currentSetting.Hide);
                        settingsChanged |= ImGui.Checkbox("Allow Tooltip", ref currentSetting.AllowTooltip);
                        settingsChanged |= ImGui.Checkbox("Allow Click Interaction", ref currentSetting.AllowClick);
                
                        ImGuiHelpers.ScaledDummy(5.0f);
                        settingsChanged |= ImGuiTweaks.ColorEditWithDefault("Color", ref currentSetting.Color, KnownColor.White.Vector());

                        ImGuiHelpers.ScaledDummy(5.0f);
                        settingsChanged |= ImGui.DragFloat("Icon Scale", ref currentSetting.Scale, 0.01f, 0.05f, 20.0f);
                    }

                    ImGui.SetCursorPosY(ImGui.GetContentRegionMax().Y - 25.0f * ImGuiHelpers.GlobalScale);
                    if (ImGui.Button("Reset to Default", new Vector2(ImGui.GetContentRegionAvail().X, 25.0f * ImGuiHelpers.GlobalScale))) {
                        currentSetting.Reset();
                        System.IconConfig.Save();
                    }
                
                    if (settingsChanged) {
                        System.IconConfig.Save();
                    }
                }
            }
        }
    }
}