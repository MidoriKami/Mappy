using System;
using System.Drawing;
using System.Linq;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;
using KamiLib.Window;
using Lumina.Excel.GeneratedSheets2;
using Map = Lumina.Excel.GeneratedSheets.Map;
using TerritoryType = Lumina.Excel.GeneratedSheets.TerritoryType;

namespace Mappy.Classes;

public class MapSelectionWindow : SelectionWindowBase<Map> {
    
    private const float Width = 133.5f;
    
    private const float Height = 75.0f;
    
    protected override bool AllowMultiSelect => false;
    
    protected override float SelectionHeight => 75.0f * ImGuiHelpers.GlobalScale;

    public MapSelectionWindow() :base(new Vector2(500.0f, 600.0f)) {
        SelectionOptions = Service.DataManager.GetExcelSheet<Map>()!
            .Where(map => map is { PlaceName.Row: not 0, TerritoryType.Value.LoadingImage: not 0 })
            .Where(map => map is not { PriorityUI: 0, PriorityCategoryUI: 0 } )
            .ToList();
    }
    
    protected override void DrawSelection(Map option) {
        using var id = ImRaii.PushId(option.RowId.ToString());

        if (option.TerritoryType.Value is null) return;
        
        DrawTerritoryImage(option.TerritoryType.Value, Service.DataManager, Service.TextureProvider);
        ImGui.SameLine();
        
        using var contentsFrame = ImRaii.Child("contents_frame", new Vector2(ImGui.GetContentRegionAvail().X, Height * ImGuiHelpers.GlobalScale), false, ImGuiWindowFlags.NoInputs);
        if (!contentsFrame) return;
        
         ImGuiHelpers.ScaledDummy(1.0f);
        
        using var table = ImRaii.Table("data_table", 2, ImGuiTableFlags.SizingStretchProp);
        if (!table) return;
        
        ImGui.TableSetupColumn("##column1", ImGuiTableColumnFlags.None, 2.0f);
        ImGui.TableSetupColumn("##column2", ImGuiTableColumnFlags.None, 1.0f);

        var placeName = option.PlaceName.Value?.Name ?? "Unknown PlaceName";
        var zoneName = option.PlaceNameSub.Value?.Name;
        var regionName = option.PlaceNameRegion.Value?.Name;
        
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(placeName);
        
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(option.RowId.ToString());
        
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
        ImGui.TextUnformatted($"{option.Id}");
    }
    
    protected override bool FilterResults(Map option, string filter) {
        if (option.PlaceNameRegion.Value?.Name.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase) ?? false) return true;
        if (option.PlaceName.Value?.Name.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase) ?? false) return true;
        if (option.PlaceNameSub.Value?.Name.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase) ?? false) return true;
        if (option.TerritoryType.Value?.Name.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase) ?? false) return true;
        if (option.Id.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase)) return true;

        return false;
    }
    
    private static void DrawTerritoryImage(TerritoryType option, IDataManager dataManager, ITextureProvider textureProvider) {
        using var imageFrame = ImRaii.Child($"image_frame{option}", ImGuiHelpers.ScaledVector2(Width * ImGuiHelpers.GlobalScale, Height), false, ImGuiWindowFlags.NoInputs);
        if (!imageFrame) return;
        
        if (dataManager.GetExcelSheet<LoadingImage>()!.GetRow(option.LoadingImage) is { } loadingImageInfo) {
            if (textureProvider.GetTextureFromGame($"ui/loadingimage/{loadingImageInfo.Unknown0}_hr1.tex") is {  } texture) {
                ImGui.Image(texture.ImGuiHandle, ImGuiHelpers.ScaledVector2(Width, Height), new Vector2(0.15f, 0.15f), new Vector2(0.85f, 0.85f));
            }
            else {
                ImGuiHelpers.ScaledDummy(Width, Height);
            }
        }
    }
}