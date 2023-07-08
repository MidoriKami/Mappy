using System;
using System.Numerics;
using ImGuiNET;

namespace Mappy.Models;

public class Viewport
{
    public Vector2 Center { get; private set; } = new(1024.0f, 1024.0f);
    public Vector2 Size { get; set; } = new(2048.0f, 2048.0f);
    public Vector2 Offset => Center * Scale - Size / 2.0f;
    public float Scale { get; private set; } = 1.0f;
    public Vector2 StartPosition { get; private set; } = Vector2.Zero;

    public void UpdateSize() => Size = ImGui.GetContentRegionAvail();
    public void UpdateViewportStart(Vector2 position) => StartPosition = position; 
    public void MoveViewportCenter(Vector2 offset) => Center += offset / Scale;
    public void SetViewportCenter(Vector2 position) => Center = position;
    public void ZoomIn(float zoomAmount) => Scale = Math.Clamp(Scale += zoomAmount, 0.15f, 6.0f);
    public void ZoomOut(float zoomAmount) => Scale = Math.Clamp(Scale -= zoomAmount, 0.15f, 6.0f);
    public void SetViewportZoom(float scale) => Scale = scale;
    public void SetImGuiDrawPosition() => ImGui.SetCursorPos(-Offset);
    public void SetImGuiDrawPosition(Vector2 position) => ImGui.SetCursorPos(-Offset + position);
    public Vector2 GetImGuiWindowDrawPosition(Vector2 position) => -Offset + position * Scale + ImGui.GetWindowPos();
}