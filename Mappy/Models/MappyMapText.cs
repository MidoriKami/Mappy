using System.Drawing;
using System.Numerics;
using Dalamud.Interface;
using Lumina.Excel.GeneratedSheets;
using Mappy.Utility;
using Action = System.Action;

namespace Mappy.Models;

public class MappyMapText {
    public string Text { get; set; } = string.Empty;
    public Vector4 TextColor { get; set; } = KnownColor.White.Vector();
    public Vector4 OutlineColor { get; set; } = KnownColor.Black.Vector();
    public Vector2? ObjectPosition { get; set; }
    public Vector2? TexturePosition { get; set; }
    public Vector4 HoverColor { get; set; }
    public Vector4 HoverOutlineColor { get; set; }
    public Action? OnClick { get; set; }
    public bool UseLargeFont { get; set; }
    
    public Vector2 GetDrawPosition(Map map) {
        if (TexturePosition is not null) return TexturePosition.Value;
        if (ObjectPosition is not null) return Position.GetTexturePosition(ObjectPosition.Value, map);
        
        return Vector2.Zero;
    }
}