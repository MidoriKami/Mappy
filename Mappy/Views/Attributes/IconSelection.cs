using System;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using Dalamud.Interface;
using ImGuiNET;
using KamiLib.AutomaticUserInterface;
using KamiLib.Caching;
using KamiLib.Utilities;

namespace Mappy.Views.Attributes;

public class IconSelection : DrawableAttribute
{
    private readonly uint[] options;
    
    public IconSelection(params uint[] options) : base(null)
    {
        this.options = options;
    }
    
    protected override void Draw(object obj, MemberInfo field, Action? saveAction = null)
    {
        var value = GetValue<uint>(obj, field);
        var totalSize = 0.0f;

        foreach (var option in options)
        {
            var icon = IconCache.Instance.GetIcon(option);
            if (icon is null) continue;
            
            var iconSize = new Vector2(icon.Width, icon.Height);
                    
            if (value == option)
            {
                var cursorPosition = ImGui.GetCursorScreenPos();
                ImGui.Image(icon.ImGuiHandle, iconSize, Vector2.Zero, Vector2.One, Vector4.One);
                ImGui.GetWindowDrawList().AddRect(cursorPosition,  cursorPosition + iconSize, ImGui.GetColorU32(KnownColor.ForestGreen.AsVector4()), 5.0f, ImDrawFlags.RoundCornersAll, 3.0f);
            }
            else
            {
                ImGui.Image(icon.ImGuiHandle, iconSize, Vector2.Zero, Vector2.One, Vector4.One * 0.5f);
            }
                    
            totalSize += iconSize.X + 7.0f * ImGuiHelpers.GlobalScale;
                    
            if (ImGui.IsItemClicked() && value != option)
            {
                SetValue(obj, field, option);
                saveAction?.Invoke();
            }

            var region = ImGui.GetContentRegionAvail();
            if (totalSize + iconSize.X < region.X)
            {
                ImGui.SameLine();
            }
            else if(totalSize + iconSize.X >= region.X)
            {
                totalSize = 0.0f;
                ImGuiHelpers.ScaledDummy(2.0f);
            }
        }
    }
}