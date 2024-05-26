using System;
using System.Collections.Generic;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;
using Mappy.Classes;
using GameObject = Dalamud.Game.ClientState.Objects.Types.GameObject;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Interface.Internal;
using Dalamud.Utility;
using Lumina.Data.Files;

namespace Mappy.MapRenderer;

public partial class MapRenderer {
    public float Scale { get; set; } = 1.0f;
    public Vector2 DrawOffset { get; set; }
    public Vector2 DrawPosition { get; private set; }

    private IDalamudTextureWrap? blendedMapTexture;
    private Task? blendTextureTask;
    private readonly Queue<BlendedTexture> textures = new(10);

    private record BlendedTexture(string TexturePath, IDalamudTextureWrap Texture);
    
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
                // ImGui.GetForegroundDrawList().AddRect(ImGui.GetWindowPos() + DrawPosition, ImGui.GetWindowPos() + DrawPosition + backgroundTexture.Size * Scale, ImGui.GetColorU32(KnownColor.Red.Vector()));
            }
        }
        else {
            // If we already have a blended texture for this path
            if (textures.FirstOrDefault(tex => tex.TexturePath == AgentMap.Instance()->SelectedMapPath.ToString()) is { Texture: var texture }) {
                blendedMapTexture = texture;
            }
            // If not, we need to generate one
            else {
                if (blendTextureTask is null) {
                    Task.Run(GenerateBlendedTexture);
                }
            }
            
            if (blendedMapTexture is not null) {
                ImGui.SetCursorPos(DrawPosition);
                ImGui.Image(blendedMapTexture.ImGuiHandle, blendedMapTexture.Size * Scale);
            }
        }
    }

    private unsafe void GenerateBlendedTexture() {
        var backgroundFile = Service.DataManager.GetFile<TexFile>($"{AgentMap.Instance()->SelectedMapBgPath.ToString()}.tex");
        var foregroundFile = Service.DataManager.GetFile<TexFile>($"{AgentMap.Instance()->SelectedMapPath.ToString()}.tex");
            
        if (backgroundFile is null || foregroundFile is null) return;
            
        var backgroundBytes = backgroundFile.GetRgbaImageData();
        var foregroundBytes = foregroundFile.GetRgbaImageData();
        var blendedBytes = new byte[backgroundBytes.Length];
            
        foreach (var index in Enumerable.Range(0, backgroundBytes.Length)) {
            blendedBytes[index] = (byte)((backgroundBytes[index] * foregroundBytes[index]) / 255);
        }
                
        var generatedTexture = Service.PluginInterface.UiBuilder.LoadImageRaw(blendedBytes, 2048, 2048, 4);

        if (textures.Count == 10) {
            textures.Dequeue();
        }
        
        textures.Enqueue(new BlendedTexture(AgentMap.Instance()->SelectedMapPath.ToString(), generatedTexture));
        blendTextureTask = null;
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