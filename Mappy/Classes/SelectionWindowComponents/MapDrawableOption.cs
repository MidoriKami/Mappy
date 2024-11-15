using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using Lumina.Excel.Sheets;

namespace Mappy.Classes.SelectionWindowComponents;

public class MapDrawableOption : DrawableOption {
    protected override void DrawIcon() {
        if (Map.RowId is 0) return;
        var option = Map.TerritoryType.Value;
        
        using var imageFrame = ImRaii.Child($"image_frame{option}", ImGuiHelpers.ScaledVector2(Width * ImGuiHelpers.GlobalScale, Height), false, ImGuiWindowFlags.NoInputs);
        if (!imageFrame) return;
        
        if (Service.DataManager.GetExcelSheet<LoadingImage>().GetRow(option.LoadingImage.RowId) is var loadingImageInfo) {
            if (Service.TextureProvider.GetFromGame($"ui/loadingimage/{loadingImageInfo.Unknown0}_hr1.tex").GetWrapOrDefault() is {  } texture) {
                ImGui.Image(texture.ImGuiHandle, ImGuiHelpers.ScaledVector2(Width, Height), new Vector2(0.15f, 0.15f), new Vector2(0.85f, 0.85f));
            }
            else {
                ImGuiHelpers.ScaledDummy(Width, Height);
            }
        }
    }
}