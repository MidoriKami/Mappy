using System.Linq;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets2;
using Map = Lumina.Excel.GeneratedSheets.Map;

namespace Mappy.Classes.SelectionWindowComponents;

public class PoiDrawableOption : DrawableOption {
    public required MapMarker MapMarker { get; set; }

    public override Vector2? MarkerLocation => new Vector2(MapMarker.X, MapMarker.Y);

    private Map? internalMap;
    
    public override Map? Map {
        get => internalMap ??= Service.DataManager.GetExcelSheet<Map>()!.FirstOrDefault(map => map.MapMarkerRange == MapMarker.RowId);
        set => internalMap = value;
    }

    public override string ExtraLineLong => MapMarker.PlaceNameSubtext.Value?.Name.ToString() ?? string.Empty;

    protected override void DrawIcon() {
        using var imageFrame = ImRaii.Child($"image_frame{MapMarker}", ImGuiHelpers.ScaledVector2(Width * ImGuiHelpers.GlobalScale, Height), false, ImGuiWindowFlags.NoInputs);
        if (!imageFrame) return;

        var xOffset = (Width - Height) / 2.0f;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + xOffset);
        ImGui.Image(Service.TextureProvider.GetFromGameIcon((uint)MapMarker.Icon).GetWrapOrEmpty().ImGuiHandle, ImGuiHelpers.ScaledVector2(Height, Height));
    }

    protected override string[] GetAdditionalFilterStrings() => [
        MapMarker.PlaceNameSubtext.Value?.Name.ToString() ?? string.Empty
    ];
}