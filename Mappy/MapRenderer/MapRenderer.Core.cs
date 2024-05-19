using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;

namespace Mappy.Classes;

public partial class MapRenderer {
    public float Scale { get; set; } = 1.0f;
    public Vector2 DrawOffset { get; set; }
    private Vector2 DrawPosition { get; set; }

    public void Draw() {
        UpdateScaleLimits();
        UpdateDrawOffset();
        
        DrawBackgroundTexture();
        DrawMapMarkers();
    }
    
    private void UpdateScaleLimits() {
        Scale = Math.Clamp(Scale, 0.05f, 20.0f);
    }

    private void UpdateDrawOffset() {
        var childCenterOffset = ImGui.GetContentRegionAvail() / 2.0f;
        var mapCenterOffset = new Vector2(1024.0f, 1024.0f) * Scale;

        DrawPosition = childCenterOffset - mapCenterOffset + DrawOffset * Scale;
    }

    private unsafe void DrawBackgroundTexture() {
        if (System.TextureCache.GetValue($"{AgentMap.Instance()->SelectedMapPath.ToString()}.tex") is { } backgroundTexture) {
            ImGui.SetCursorPos(DrawPosition);
            ImGui.Image(backgroundTexture.ImGuiHandle, backgroundTexture.Size * Scale);
        }
    }

    private void DrawMapMarkers() {
        DrawStaticMapMarkers();
        DrawDynamicMarkers();
        DrawGameObjects();
        DrawTemporaryMarkers();
        DrawGatheringMarkers();
        DrawFieldMarkers();
        DrawPlayer();
    }
}