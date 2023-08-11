using System.Drawing;
using System.Numerics;
using KamiLib.Utilities;
using Lumina.Excel.GeneratedSheets;
using Mappy.Utility;

namespace Mappy.Models;

public class MappyMapText
{
    public string Text { get; set; } = string.Empty;
    public Vector4 TextColor { get; set; } = KnownColor.White.AsVector4();
    public Vector4 OutlineColor { get; set; } = KnownColor.Black.AsVector4();
    public Vector2 ObjectPosition { get; set; } = Vector2.Zero;
    
    public Vector2 GetDrawPosition(Map map) => Position.GetTexturePosition(ObjectPosition, map);
}