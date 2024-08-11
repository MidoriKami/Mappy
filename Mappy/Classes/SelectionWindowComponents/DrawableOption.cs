using System.Drawing;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace Mappy.Classes.SelectionWindowComponents;

public abstract class DrawableOption {
    protected virtual string[] GetAdditionalFilterStrings() => [];

    public virtual Map? Map { get; set; }
    
    protected static float Width => 133.5f * ImGuiHelpers.GlobalScale;
    
    protected static float Height => 75.0f * ImGuiHelpers.GlobalScale;

    protected abstract void DrawIcon();

    public virtual string ExtraLineLong => string.Empty;
    
    public virtual string ExtraLineShort => string.Empty;

    public virtual Vector2? MarkerLocation => null;
    
    public string[] GetFilterStrings() {
        var baseStrings = new[] { 
            Map?.PlaceNameRegion.Value?.Name.ToString() ?? string.Empty,
            Map?.PlaceName.Value?.Name.ToString() ?? string.Empty,
            Map?.PlaceNameSub.Value?.Name.ToString() ?? string.Empty,
            Map?.TerritoryType.Value?.Name.ToString() ?? string.Empty,
            Map?.Id.ToString() ?? string.Empty,
        };

        return baseStrings.Concat(GetAdditionalFilterStrings()).ToArray();
    }

    public void Draw() {
        if (Map is null) return;
        
        using var id = ImRaii.PushId(Map.RowId.ToString());
        
        DrawIcon();
        ImGui.SameLine();
        
        using var contentsFrame = ImRaii.Child("contents_frame", new Vector2(ImGui.GetContentRegionAvail().X, Height * ImGuiHelpers.GlobalScale), false, ImGuiWindowFlags.NoInputs);
        if (!contentsFrame) return;
        
        ImGuiHelpers.ScaledDummy(1.0f);
        
        using var table = ImRaii.Table("data_table", 2, ImGuiTableFlags.SizingStretchProp);
        if (!table) return;
        
        ImGui.TableSetupColumn("##column1", ImGuiTableColumnFlags.None, 2.0f);
        ImGui.TableSetupColumn("##column2", ImGuiTableColumnFlags.None, 1.0f);

        var placeName = Map.PlaceName.Value?.Name ?? "Unknown PlaceName";
        var zoneName = Map.PlaceNameSub.Value?.Name;
        var regionName = Map.PlaceNameRegion.Value?.Name;
        
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(placeName);
        
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(Map.RowId.ToString());
        
        ImGui.TableNextRow();
        ImGui.TableNextColumn();

        using var grayColor = ImRaii.PushColor(ImGuiCol.Text, KnownColor.DarkGray.Vector());
        if (zoneName is not null && !zoneName.ToString().IsNullOrEmpty() && regionName is not null && !regionName.ToString().IsNullOrEmpty()) {
            ImGui.TextUnformatted($"{regionName}, {zoneName}");
        }
        else if (zoneName is not null && !zoneName.ToString().IsNullOrEmpty()) {
            ImGui.TextUnformatted($"{zoneName}");
        }
        else if (regionName is not null && !regionName.ToString().IsNullOrEmpty()) {
            ImGui.TextUnformatted($"{regionName}");
        }

        ImGui.TableNextColumn();
        ImGui.TextUnformatted($"{Map.Id}");
        
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(ExtraLineLong);
        
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(ExtraLineShort);
    }
}