using System.Numerics;
using ImGuiScene;
using KamiLib.Caching;

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

    public TextureWrap? IconTexture => IconCache.Instance.GetIcon(IconId);
    public Vector2 IconSize => IconTexture is null ? Vector2.Zero : new Vector2(IconTexture.Width, IconTexture.Height);
}
