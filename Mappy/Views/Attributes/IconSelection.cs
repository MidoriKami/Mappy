using System;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using ImGuiNET;
using KamiLib.AutomaticUserInterface;

namespace Mappy.Views.Attributes;

// Todo: Maybe add icon search idk.
public class IconSelection : DrawableAttribute {
    private readonly uint[] options;
    
    public IconSelection(params uint[] options) : base(null) {
        this.options = options;
    }
    
    protected override void Draw(object obj, MemberInfo field, Action? saveAction = null) {
        var value = GetValue<uint>(obj, field);
        var totalSize = 0.0f;

        foreach (var option in options) {
            if (Service.TextureProvider.GetIcon(option) is not {} icon) continue;
            
            if (value == option) {
                var cursorPosition = ImGui.GetCursorScreenPos();
                ImGui.Image(icon.ImGuiHandle, icon.Size, Vector2.Zero, Vector2.One, Vector4.One);
                ImGui.GetWindowDrawList().AddRect(cursorPosition,  cursorPosition + icon.Size, ImGui.GetColorU32(KnownColor.ForestGreen.Vector()), 5.0f, ImDrawFlags.RoundCornersAll, 3.0f);
            } else {
                ImGui.Image(icon.ImGuiHandle, icon.Size, Vector2.Zero, Vector2.One, Vector4.One * 0.5f);
            }
                    
            totalSize += icon.Width + 7.0f * ImGuiHelpers.GlobalScale;
                    
            if (ImGui.IsItemClicked() && value != option) {
                SetValue(obj, field, option);
                saveAction?.Invoke();
            }

            var region = ImGui.GetContentRegionAvail();
            if (totalSize + icon.Width < region.X) {
                ImGui.SameLine();
            } else if(totalSize + icon.Width >= region.X) {
                totalSize = 0.0f;
                ImGuiHelpers.ScaledDummy(2.0f);
            }
        }
    }
}