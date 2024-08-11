using System.Linq;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace Mappy.Classes.SelectionWindowComponents;

public class AetheryteDrawableOption : DrawableOption {
    public required Aetheryte Aetheryte { get; set; }

    public override string ExtraLineLong => GetName();

    private Map? internalMap;
    
    public override Map? Map {
        get => internalMap ??= GetAetheryteMap();
        set => internalMap = value;
    }

    protected override string[] GetAdditionalFilterStrings() => [
        Aetheryte.PlaceName.Value?.Name ?? string.Empty,
        Aetheryte.AethernetName.Value?.Name ?? string.Empty,
    ];

    protected override void DrawIcon() {
        using var imageFrame = ImRaii.Child($"image_frame{Aetheryte}", ImGuiHelpers.ScaledVector2(Width * ImGuiHelpers.GlobalScale, Height), false, ImGuiWindowFlags.NoInputs);
        if (!imageFrame) return;

        var xOffset = (Width - Height) / 2.0f;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + xOffset);
        ImGui.Image(Service.TextureProvider.GetFromGameIcon(Aetheryte.IsAetheryte ? 60453 : 60430).GetWrapOrEmpty().ImGuiHandle, ImGuiHelpers.ScaledVector2(Height, Height));
    }
    
    private Map? GetAetheryteMap() {
        if (Aetheryte.Map.Row is not 0) return Aetheryte.Map.Value;

        if (Service.DataManager.GetExcelSheet<Aetheryte>()!.FirstOrDefault(aetheryte => aetheryte.IsAetheryte && aetheryte.AethernetGroup == Aetheryte.AethernetGroup) is not { } targetAetheryte) return null;

        return targetAetheryte.Map.Value;
    }

    private string GetName() {
        if (Aetheryte.AethernetName.Row is not 0) return Aetheryte.AethernetName.Value?.Name.ToString() ?? string.Empty;
        if (Aetheryte.PlaceName.Row is not 0) return Aetheryte.PlaceName.Value?.Name.ToString() ?? string.Empty;

        return string.Empty;
    }
}