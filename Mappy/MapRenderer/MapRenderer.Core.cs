using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;
using Mappy.Classes;
using GameObject = Dalamud.Game.ClientState.Objects.Types.GameObject;

namespace Mappy.MapRenderer;

public partial class MapRenderer {
    public float Scale { get; set; } = 1.0f;
    public Vector2 DrawOffset { get; set; }
    public Vector2 DrawPosition { get; private set; }

    public void CenterOnGameObject(GameObject obj) 
        => DrawOffset = -new Vector2(obj.Position.X, obj.Position.Z) * DrawHelpers.GetMapScaleFactor();

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
        var mapCenterOffset = new Vector2(1024.0f, 1024.0f) * Scale ;

        DrawPosition = childCenterOffset - mapCenterOffset + DrawOffset * Scale;
    }

    private unsafe void DrawBackgroundTexture() {
        if (System.TextureCache.GetValue($"{AgentMap.Instance()->SelectedMapPath.ToString()}.tex") is { } backgroundTexture) {
            ImGui.SetCursorPos(DrawPosition);
            ImGui.Image(backgroundTexture.ImGuiHandle, backgroundTexture.Size * Scale);
            // ImGui.GetForegroundDrawList().AddRect(ImGui.GetWindowPos() + DrawPosition, ImGui.GetWindowPos() + DrawPosition + backgroundTexture.Size * Scale, ImGui.GetColorU32(KnownColor.Red.Vector()));
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
        DrawFlag();
    }
}