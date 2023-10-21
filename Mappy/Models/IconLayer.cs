using System.Numerics;
using Dalamud.Interface.Internal;

namespace Mappy.Models;

public class IconLayer {
    public IconLayer(uint iconId, Vector2 offsetPosition) {
        IconId = iconId;
        PositionOffset = offsetPosition;
    }

    private uint _iconId { get; set; }

    public uint IconId {
        get => _iconId;
        set {
            _iconId = value;
            texture = null;
        }
    }

    public Vector2 PositionOffset { get; set; }
    private IDalamudTextureWrap? texture;
    public IDalamudTextureWrap? IconTexture => texture ??= Service.TextureProvider.GetIcon(IconId);
}
