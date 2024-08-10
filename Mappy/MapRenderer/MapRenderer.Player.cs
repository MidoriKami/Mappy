using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;
using Mappy.Classes;
using Mappy.Extensions;

namespace Mappy.MapRenderer;

public partial class MapRenderer {
    private unsafe void DrawPlayer() {
        if (AgentMap.Instance()->SelectedMapId != AgentMap.Instance()->CurrentMapId) return;

        if (Service.ClientState.LocalPlayer is { } localPlayer) {
            var position = ImGui.GetWindowPos() +
                           DrawPosition +
                           (localPlayer.GetMapPosition() -
                           DrawHelpers.GetMapOffsetVector() +
                           DrawHelpers.GetMapCenterOffsetVector()) * Scale;
            
            DrawLookLine(position);
            DrawPlayerIcon(position);
        }
    }
    
    private void DrawLookLine(Vector2 position) {
        var angle = GetCameraRotation();

        var lineLength = System.SystemConfig.ConeSize * (System.SystemConfig.ScalePlayerCone ? 1.0f : Scale);
        var halfConeAngle = DegreesToRadians(90.0f) / 2.0f;
        
        DrawAngledLineFromCenter(position, lineLength, angle - halfConeAngle);
        DrawAngledLineFromCenter(position, lineLength, angle + halfConeAngle);
        DrawLineArcFromCenter(position, lineLength, angle);
        
        DrawFilledSemiCircle(position, lineLength, angle);
    }

    private static void DrawAngledLineFromCenter(Vector2 center, float lineLength, float angle) {
        var lineSegment = new Vector2(lineLength * MathF.Cos(angle), lineLength * MathF.Sin(angle));
        var coneOutlineColor = ImGui.GetColorU32(System.SystemConfig.PlayerConeOutlineColor);
        ImGui.GetWindowDrawList().AddLine(center, center + lineSegment, coneOutlineColor, 3.0f);
    }

    private static void DrawLineArcFromCenter(Vector2 center, float distance, float rotation) {
        var halfConeAngle = DegreesToRadians(90.0f) / 2.0f;
        var coneOutlineColor = ImGui.GetColorU32(System.SystemConfig.PlayerConeOutlineColor);

        var start = rotation - halfConeAngle;
        var stop = rotation + halfConeAngle;
        
        ImGui.GetWindowDrawList().PathArcTo(center, distance, start, stop);
        ImGui.GetWindowDrawList().PathStroke(coneOutlineColor, ImDrawFlags.None, 3.0f);
    }

    private static void DrawFilledSemiCircle(Vector2 center, float distance, float rotation) {
        var halfConeAngle = DegreesToRadians(90.0f) / 2.0f;
        
        var coneColor = ImGui.GetColorU32(System.SystemConfig.PlayerConeColor);
        var startAngle = rotation - halfConeAngle;
        var stopAngle = rotation + halfConeAngle;
        
        var startPosition = new Vector2(distance * MathF.Cos(rotation - halfConeAngle), distance * MathF.Sin(rotation - halfConeAngle));

        ImGui.GetWindowDrawList().PathArcTo(center, distance, startAngle, stopAngle);
        ImGui.GetWindowDrawList().PathLineTo(center);
        ImGui.GetWindowDrawList().PathLineTo(center + startPosition);
        ImGui.GetWindowDrawList().PathFillConvex(coneColor);
    }
    
    private static unsafe float GetCameraRotation() 
        => -DegreesToRadians(AddonAreaMap.AddonAreaMapNumberArray.Instance()->ConeRotation) - 0.5f * MathF.PI;

    private static float DegreesToRadians(float degrees) 
        => MathF.PI / 180.0f * degrees;

    private void DrawPlayerIcon(Vector2 position) {
        if (!System.SystemConfig.ShowPlayerIcon) return;
        if (Service.ClientState is not { LocalPlayer: { } player }) return;
        
        var texture = Service.TextureProvider.GetFromGameIcon(60443).GetWrapOrEmpty();
        var angle = -player.Rotation + MathF.PI / 2.0f;

        var scale = System.SystemConfig.ScaleWithZoom ? Scale : 1.0f;
        scale *= System.SystemConfig.PlayerIconScale;

        var vectors = GetRotationVectors(angle, position, texture.Size / 2.0f * scale);
    
        ImGui.GetWindowDrawList().AddImageQuad(texture.ImGuiHandle, vectors[0], vectors[1], vectors[2], vectors[3]);
    }
    
    private static Vector2[] GetRotationVectors(float angle, Vector2 center, Vector2 size) {
        var cosA = MathF.Cos(angle + 0.5f * MathF.PI);
        var sinA = MathF.Sin(angle + 0.5f * MathF.PI);
    
        Vector2[] vectors = [
            center + ImRotate(new Vector2(-size.X * 0.5f, -size.Y * 0.5f), cosA, sinA),
            center + ImRotate(new Vector2(+size.X * 0.5f, -size.Y * 0.5f), cosA, sinA),
            center + ImRotate(new Vector2(+size.X * 0.5f, +size.Y * 0.5f), cosA, sinA),
            center + ImRotate(new Vector2(-size.X * 0.5f, +size.Y * 0.5f), cosA, sinA),
        ];
        return vectors;
    }
    
    private static Vector2 ImRotate(Vector2 v, float cosA, float sinA) 
        => new(v.X * cosA - v.Y * sinA, v.X * sinA + v.Y * cosA);
}