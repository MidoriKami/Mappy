using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using KamiLib.Extensions;
using Lumina.Excel.Sheets;

namespace Mappy.Classes.SelectionWindowComponents;

public class AetheryteDrawableOption : DrawableOption
{
    public required Aetheryte Aetheryte { get; set; }

    public override string ExtraLineLong => GetName();

    public override Map Map => GetAetheryteMap()!.Value; // Probably a bad idea

    protected override string[] GetAdditionalFilterStrings() =>
    [
        Aetheryte.PlaceName.Value.Name.ExtractText(),
        Aetheryte.AethernetName.Value.Name.ExtractText(),
    ];

    protected override void DrawIcon()
    {
        using var imageFrame = ImRaii.Child($"image_frame{Aetheryte.RowId}#{MarkerLocation}#{ExtraLineLong}", new Vector2(Width, Height), false, ImGuiWindowFlags.NoInputs);
        if (!imageFrame) return;

        var xOffset = (Width - Height) / 2.0f;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + xOffset);
        ImGui.Image(Service.TextureProvider.GetFromGameIcon(Aetheryte.IsAetheryte ? 60453 : 60430).GetWrapOrEmpty().Handle, new Vector2(Height, Height));
    }

    private Map? GetAetheryteMap()
    {
        if (Aetheryte.Map.RowId is not 0) return Aetheryte.Map.Value;

        if (Service.DataManager.GetExcelSheet<Aetheryte>().FirstOrNull(aetheryte => aetheryte.IsAetheryte && aetheryte.AethernetGroup == Aetheryte.AethernetGroup) is not
            { } targetAetheryte)
            return null;

        return targetAetheryte.Map.Value;
    }

    private string GetName()
    {
        if (Aetheryte.AethernetName.RowId is not 0) return Aetheryte.AethernetName.Value.Name.ExtractText();
        if (Aetheryte.PlaceName.RowId is not 0) return Aetheryte.PlaceName.Value.Name.ExtractText();

        return string.Empty;
    }

    public override string GetElementKey() => base.GetElementKey() + $"{Aetheryte.RowId}";
}