using System;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;
using Mappy.Classes;
using GameObject = Dalamud.Game.ClientState.Objects.Types.GameObject;
using Dalamud.Interface.Internal;
using Dalamud.Utility;
using Lumina.Data.Files;

namespace Mappy.MapRenderer;

public partial class MapRenderer {
    public float Scale { get; set; } = 1.0f;
    public Vector2 DrawOffset { get; set; }
    public Vector2 DrawPosition { get; private set; }

    private IDalamudTextureWrap? blendedTexture;
    private string blendedPath = string.Empty;

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
        if (AgentMap.Instance()->SelectedMapBgPath.Length is 0) {
            if (System.TextureCache.GetValue($"{AgentMap.Instance()->SelectedMapPath.ToString()}.tex") is { } backgroundTexture) {
                ImGui.SetCursorPos(DrawPosition);
                ImGui.Image(backgroundTexture.ImGuiHandle, backgroundTexture.Size * Scale);
            }
        }
        else {
            if (blendedPath != AgentMap.Instance()->SelectedMapBgPath.ToString()) {
                blendedTexture = LoadTexture();
                blendedPath = AgentMap.Instance()->SelectedMapBgPath.ToString();
            }

            if (blendedTexture is not null) {
                ImGui.SetCursorPos(DrawPosition);
                ImGui.Image(blendedTexture.ImGuiHandle, blendedTexture.Size * Scale);
            }
        }
    }

    private unsafe IDalamudTextureWrap? LoadTexture() {
        var backgroundFile = Service.DataManager.GetFile<TexFile>($"{AgentMap.Instance()->SelectedMapBgPath.ToString()}.tex");
        var foregroundFile = Service.DataManager.GetFile<TexFile>($"{AgentMap.Instance()->SelectedMapPath.ToString()}.tex");
        if (backgroundFile is null || foregroundFile is null) return null;

        var backgroundBytes = backgroundFile.GetRgbaImageData();
        var foregroundBytes = foregroundFile.GetRgbaImageData();

        var length = backgroundBytes.Length;
        var vectorSize = Vector256<byte>.Count;

        Parallel.For(0, length / vectorSize, i => {
            var start = i * vectorSize;
            fixed (byte* fgPtr = foregroundBytes, bgPtr = backgroundBytes) {
                // Load vectors from background and foreground bytes
                var bgVector = Avx.LoadVector256(bgPtr + start);
                var fgVector = Avx.LoadVector256(fgPtr + start);

                // Convert to float vectors for better precision
                var backgroundFloat = bgVector.AsSingle() / 255f;
                var foregroundFloat = fgVector.AsSingle() / 255f;
            
                // Multiply and convert back to byte vectors
                var resultVector = (backgroundFloat * foregroundFloat * 255f).AsByte();

                // Store the result back to the background bytes
                Avx.Store(bgPtr + start, resultVector);
            }
        });

        return Service.PluginInterface.UiBuilder.LoadImageRaw(backgroundBytes, 2048, 2048, 4);
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