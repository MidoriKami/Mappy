using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Dalamud.Interface;
using ImGuiNET;
using KamiLib.AutomaticUserInterface;
using KamiLib.Caching;
using KamiLib.Utilities;
using Lumina.Excel.GeneratedSheets;
using Action = System.Action;

namespace Mappy.Views.Attributes;

public class MapMarkerSelection : DrawableAttribute
{
    public MapMarkerSelection() : base(null) { }
    
    protected override void Draw(object obj, MemberInfo field, Action? saveAction = null)
    {
        var disabledMarkers = GetValue<HashSet<uint>>(obj, field);
        var allMarkers = LuminaCache<MapSymbol>.Instance
            .Where(marker => marker.Icon is not 0)
            .Select(markers => markers.Icon);

        var totalSize = 0.0f;
        var remainingRegion = ImGui.GetContentRegionAvail();

        var childSize = remainingRegion.Y < 250.0f ? new Vector2(0, 250.0f * ImGuiHelpers.GlobalScale) : Vector2.Zero;
        
        if (ImGui.BeginChild("##MapMarkerSelectChild", childSize, false, ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar))
        {
            foreach (uint iconId in allMarkers)
            {
                var iconTexture = IconCache.Instance.GetIcon(iconId);
                if (iconTexture is null) continue;

                var iconSize = new Vector2(iconTexture.Width, iconTexture.Height);
                var cursorPosition = ImGui.GetCursorScreenPos();

                var disabled = disabledMarkers.Contains(iconId);
                var knownColor = disabled ? KnownColor.Red : KnownColor.ForestGreen;
                var borderColor = ImGui.GetColorU32( knownColor.AsVector4() );
            
                ImGui.Image(iconTexture.ImGuiHandle, iconSize);
                ImGui.GetWindowDrawList().AddRect(cursorPosition, cursorPosition + iconSize, borderColor, 5.0f, ImDrawFlags.RoundCornersAll, 3.0f);

                totalSize += iconSize.X + 7.0f;
                    
                if (ImGui.IsItemClicked())
                {
                    if (disabled)
                    {
                        disabledMarkers.Remove(iconId);
                    }
                    else
                    {
                        disabledMarkers.Add(iconId);
                    }
                    SetValue(obj, field, disabledMarkers);
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
        
        ImGui.EndChild();
    }
}