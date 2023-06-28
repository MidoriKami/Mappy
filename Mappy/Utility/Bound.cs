using System.Numerics;
using ImGuiNET;

namespace Mappy.Utility;

public class Bound
{
    public static bool IsCursorInWindow()
    {
        var windowStart = ImGui.GetWindowPos();
        var windowSize = ImGui.GetWindowSize();

        return Bound.IsBoundedBy(ImGui.GetMousePos(), windowStart, windowStart + windowSize);
    }
    
    public static bool IsCursorInWindowHeader()
    {
        var windowStart = ImGui.GetWindowPos();
        var headerSize = ImGui.GetWindowSize() with { Y = ImGui.GetWindowContentRegionMin().Y };
        
        return Bound.IsBoundedBy(ImGui.GetMousePos(), windowStart, windowStart + headerSize);
    }
    
    public static bool IsBoundedBy(Vector2 cursor, Vector2 minBounds, Vector2 maxBounds)
    {
        if (cursor.X >= minBounds.X && cursor.Y >= minBounds.Y)
        {
            if (cursor.X <= maxBounds.X && cursor.Y <= maxBounds.Y)
            {
                return true;
            }
        }

        return false;
    }
}