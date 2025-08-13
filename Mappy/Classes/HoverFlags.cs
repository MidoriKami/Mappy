using System;

namespace Mappy.Classes;

[Flags]
public enum HoverFlags
{
    Nothing = 0,
    MapTexture = 1 << 0,
    Toolbar = 1 << 1,
    CoordinateBar = 1 << 2,
    Window = 1 << 3,
    WindowInnerFrame = 1 << 4,
}

public static class HoverFlagsExtensions
{
    public static bool Any(this HoverFlags flags) => flags != HoverFlags.Nothing;
}