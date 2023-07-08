using System.Numerics;
using Mappy.Models.Enums;

namespace Mappy.Models;

public class TemporaryMapMarker
{
    public MarkerType Type { get; init; } = MarkerType.Unknown;
    public uint MapID { get; set; }
    public uint IconID { get; init; }
    public Vector2 Position { get; init; } = Vector2.Zero;
    public float Radius { get; init; }
    public string TooltipText { get; init; } = string.Empty;
}