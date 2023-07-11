using ImGuiNET;

namespace Mappy.Utility;

public static class DrawFlags
{
    public const ImGuiWindowFlags DefaultFlags = 
        ImGuiWindowFlags.NoFocusOnAppearing |
        ImGuiWindowFlags.NoNav |
        ImGuiWindowFlags.NoBringToFrontOnFocus |
        ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse |
        ImGuiWindowFlags.NoDocking;

    public const ImGuiWindowFlags NoDecorationFlags =
        ImGuiWindowFlags.NoDecoration |
        ImGuiWindowFlags.NoBackground;

    public const ImGuiWindowFlags NoMoveResizeFlags =
        ImGuiWindowFlags.NoMove |
        ImGuiWindowFlags.NoResize;
}