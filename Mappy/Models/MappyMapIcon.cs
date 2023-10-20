using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Internal;
using Lumina.Excel.GeneratedSheets;
using Mappy.Utility;
using Action = System.Action;

namespace Mappy.Models;

// Todo: Add option to animate ring
public class MappyMapIcon {
    public uint IconId { get; set; }
    public List<IconLayer> Layers { get; set; } = new();
    
    public float Radius { get; set; }
    public Vector4 RadiusColor { get; set; } = KnownColor.Aqua.Vector();

    public string Tooltip { get; set; } = string.Empty;
    public Func<string>? GetTooltipFunc { get; set; }
    public string GetTooltip() => GetTooltipFunc is not null ? GetTooltipFunc.Invoke() : Tooltip;

    public uint TooltipExtraIcon { get; set; }
    public string TooltipExtraText { get; set; } = string.Empty;
    public Func<string>? GetTooltipExtraTextFunc { get; set; }
    public string GetTooltipExtraText() => GetTooltipExtraTextFunc is not null ? GetTooltipExtraTextFunc.Invoke() : TooltipExtraText;
    public Func<Vector4>? GetTooltipColorFunc { get; set; }

    public float VerticalPosition { get; set; }

    public Vector2? TexturePosition { get; set; }
    public Vector2? ObjectPosition { get; set; }
    
    public Vector4? ColorManipulation { get; set; }

    public Vector2 GetDrawPosition(Map map) {
        if (TexturePosition is not null) return TexturePosition.Value;
        if (ObjectPosition is not null) return Position.GetTexturePosition(ObjectPosition.Value, map);
        
        return Vector2.Zero;
    }
    
    public Action? OnClickAction { get; set; }

    public IDalamudTextureWrap? IconTexture => Service.TextureProvider.GetIcon(IconId);
}