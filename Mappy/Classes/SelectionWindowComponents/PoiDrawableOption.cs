using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Lumina.Excel.Sheets;

namespace Mappy.Classes.SelectionWindowComponents;

public class PoiDrawableOption : DrawableOption {
    public required MapMarker MapMarker { get; set; }

    public override Vector2? MarkerLocation => new Vector2(MapMarker.X, MapMarker.Y);

    public override Map Map => Service.DataManager.GetExcelSheet<Map>().FirstOrDefault(map => map.MapMarkerRange == MapMarker.RowId);

    public override string ExtraLineLong => MapMarker.PlaceNameSubtext.Value.Name.ExtractText();

    protected override void DrawIcon() {
        using var imageFrame = ImRaii.Child($"image_frame{MapMarker.RowId}#{MarkerLocation}", new Vector2(Width, Height), false, ImGuiWindowFlags.NoInputs);
        if (!imageFrame) return;

        var xOffset = (Width - Height) / 2.0f;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + xOffset);
        ImGui.Image(Service.TextureProvider.GetFromGameIcon((uint) MapMarker.Icon).GetWrapOrEmpty().Handle, new Vector2(Height, Height));
    }

    protected override string[] GetAdditionalFilterStrings() => [
        MapMarker.PlaceNameSubtext.Value.Name.ExtractText(),
    ];

    public override string GetElementKey()
        => base.GetElementKey() + $"{MapMarker.RowId}";

}