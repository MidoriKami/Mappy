using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Lumina.Excel.Sheets;

namespace Mappy.Classes.SelectionWindowComponents;

public class MapDrawableOption : DrawableOption
{
    protected override void DrawIcon()
    {
        var option = Map.TerritoryType.Value;

        using var imageFrame = ImRaii.Child($"image_frame{option}", new Vector2(Width, Height), false, ImGuiWindowFlags.NoInputs);
        if (!imageFrame) return;

        var texture = GetMapTexture(Map.RowId);
        if (texture is not null) {
            ImGui.Image(texture.Handle, new Vector2(Width, Height), new Vector2(0.15f, 0.15f), new Vector2(0.85f, 0.85f));
        }
        else {
            ImGuiHelpers.ScaledDummy(Width, Height);
        }
    }

    public static IDalamudTextureWrap? GetMapTexture(uint mapId)
    {
        if (mapId is 0) return null;

        var map = Service.DataManager.GetExcelSheet<Map>().GetRow(mapId);
        var territory = map.TerritoryType;
        if (!territory.IsValid) return null;

        var loadingImage = territory.Value.LoadingImage;
        if (!loadingImage.IsValid) return null;

        var texturePath = $"ui/loadingimage/{loadingImage.Value.FileName}_hr1.tex";
        return Service.TextureProvider.GetFromGame(texturePath).GetWrapOrDefault();
    }
}