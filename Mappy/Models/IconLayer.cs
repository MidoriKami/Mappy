using System.Numerics;
using Dalamud.Interface.Internal;

namespace Mappy.Models;

public class IconLayer
{
    public IconLayer(uint iconId, Vector2 offsetPosition)
    {
        IconId = iconId;
        PositionOffset = offsetPosition;
    }
    
    public uint IconId { get; set; }
    public Vector2 PositionOffset { get; set; }
    public IDalamudTextureWrap? IconTexture => Service.TextureProvider.GetIcon(IconId);
}
