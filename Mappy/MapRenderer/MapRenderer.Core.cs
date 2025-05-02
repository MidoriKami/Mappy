using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;
using Mappy.Classes;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Data.Files;

namespace Mappy.MapRenderer;

public partial class MapRenderer {
    public static float Scale {
        get => System.SystemConfig.MapScale;
        set => System.SystemConfig.MapScale = value;
    }

    public Vector2 DrawOffset { get; set; }
    public Vector2 DrawPosition { get; private set; }

    private IDalamudTextureWrap? blendedTexture;
    private IDalamudTextureWrap? fogTexture;
    private int lastKnownDiscoveryFlags;
    private string blendedPath = string.Empty;

    public void CenterOnGameObject(IGameObject obj) 
        => DrawOffset = -new Vector2(obj.Position.X, obj.Position.Z) * DrawHelpers.GetMapScaleFactor() + DrawHelpers.GetMapOffsetVector();

    public void Draw() {
        UpdateScaleLimits();
        UpdateDrawOffset();
        
        DrawBackgroundTexture();
        DrawFogOfWar();
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
            var texture = Service.TextureProvider.GetFromGame($"{AgentMap.Instance()->SelectedMapPath.ToString()}.tex").GetWrapOrEmpty();
            
            ImGui.SetCursorPos(DrawPosition);
            ImGui.Image(texture.ImGuiHandle, texture.Size * Scale);
        }
        else {
            if (blendedPath != AgentMap.Instance()->SelectedMapBgPath.ToString()) {
                blendedTexture?.Dispose();
                blendedTexture = LoadTexture();
                blendedPath = AgentMap.Instance()->SelectedMapBgPath.ToString();
            }

            if (blendedTexture is not null) {
                ImGui.SetCursorPos(DrawPosition);
                ImGui.Image(blendedTexture.ImGuiHandle, blendedTexture.Size * Scale);
            }
        }
    }

    private unsafe void DrawFogOfWar() {
        if (!System.SystemConfig.ShowFogOfWar) return;
        
        var areaMapNumberArray = AtkStage.Instance()->GetNumberArrayData(NumberArrayType.AreaMap2);

        if (areaMapNumberArray->IntArray[2] != lastKnownDiscoveryFlags) {
            lastKnownDiscoveryFlags = areaMapNumberArray->IntArray[2];
            Service.Log.Debug($"Updating Discovery Flags {lastKnownDiscoveryFlags:X}");

            if (lastKnownDiscoveryFlags != -1) {
                    fogTexture = LoadFogTexture();
            }
            else {
                Service.Log.Debug($"Skipping Update");
            }
        }

        if (fogTexture is not null && lastKnownDiscoveryFlags != -1) {
            ImGui.SetCursorPos(DrawPosition);
            ImGui.Image(fogTexture.ImGuiHandle, fogTexture.Size * Scale);
        }
    }
    
    private static unsafe IDalamudTextureWrap? LoadTexture() {
        var vanillaBgPath = $"{AgentMap.Instance()->SelectedMapBgPath.ToString()}.tex";
        var vanillaFgPath = $"{AgentMap.Instance()->SelectedMapPath.ToString()}.tex";
        
        var bgFile = GetTexFile(vanillaBgPath);
        var fgFile = GetTexFile(vanillaFgPath);

        if (bgFile is null || fgFile is null) {
            Service.Log.Warning("Failed to load map textures");
            return null;
        }

        var backgroundBytes = bgFile.GetRgbaImageData();
        var foregroundBytes = fgFile.GetRgbaImageData();

        // Blend textures together
        Parallel.For(0, 2048 * 2048, i => {
            var index = i * 4;
            
            // Blend, R, G, B, skip A.
            backgroundBytes[index + 0] = (byte)(backgroundBytes[index + 0] * foregroundBytes[index + 0] / 255);
            backgroundBytes[index + 1] = (byte)(backgroundBytes[index + 1] * foregroundBytes[index + 1] / 255);
            backgroundBytes[index + 2] = (byte)(backgroundBytes[index + 2] * foregroundBytes[index + 2] / 255);
        });

        return Service.TextureProvider.CreateFromRaw(RawImageSpecification.Rgba32(2048, 2048), backgroundBytes);
    }

    private unsafe IDalamudTextureWrap? LoadFogTexture() {
        var fogTexturePath = $"{AgentMap.Instance()->SelectedMapBgPath.ToString().TrimEnd('_', 'm')}d.tex";
        var vanillaBgPath = $"{AgentMap.Instance()->SelectedMapBgPath.ToString()}.tex";
        
        var fogTextureFile = GetTexFile(fogTexturePath);
        var bgFile = GetTexFile(vanillaBgPath);

        if (bgFile is null || fogTextureFile is null) {
            Service.Log.Warning("Failed to load map textures");
            return null;
        }
        
        var backgroundBytes = bgFile.GetRgbaImageData();
        var fogTextureBytes = fogTextureFile.GetRgbaImageData();
        
        foreach (var xPageIndex in Enumerable.Range(0, 4))
        foreach (var yPageIndex in Enumerable.Range(0, 3))
        foreach (var color in Enumerable.Range(0, 3)) {
                    
            // If this visibility flag is set
            var currentBitIndex = (xPageIndex * 3 + yPageIndex * 12 + color);
            if (currentBitIndex >= 32) continue;

            if ((lastKnownDiscoveryFlags & (1 << currentBitIndex)) != 0) {
                        
                Service.Log.Debug($"Flag {currentBitIndex} is Set, Revealing [ {xPageIndex:00}, {yPageIndex:00} ] Color [ {color} ]");

                Parallel.For(0, 128, x => {
                    Parallel.For(0, 128, y => {
                        var pixelIndex = (x + y * 512) * 4 + xPageIndex * 128 * 4 + yPageIndex * 512 * 4;
                        var targetPixel = (x + 2048 * y) * 4;

                        var alphaValue = color switch {
                            0 => fogTextureBytes[pixelIndex + 0],
                            1 => fogTextureBytes[pixelIndex + 1],
                            2 => fogTextureBytes[pixelIndex + 2],
                            _ => throw new ArgumentOutOfRangeException(),
                        };

                        const int scaleFactor = 16;
                        foreach (var xScalar in Enumerable.Range(0, scaleFactor))
                        foreach (var yScalar in Enumerable.Range(0, scaleFactor)) {
                            var scalingPixelTarget = targetPixel * scaleFactor + xScalar * 4 + yScalar * 2048 * 4;

                            backgroundBytes[scalingPixelTarget + 3] = Math.Min(backgroundBytes[scalingPixelTarget + 3], (byte)(255 - alphaValue));
                        }
                    });
                });
            }
        }

        return Service.TextureProvider.CreateFromRaw(RawImageSpecification.Rgba32(2048, 2048), backgroundBytes);
    }

    private static TexFile? GetTexFile(string rawPath) {
        var path = Service.TextureSubstitutionProvider.GetSubstitutedPath(rawPath);

        if (Path.IsPathRooted(path)) {
            return Service.DataManager.GameData.GetFileFromDisk<TexFile>(path);
        }

        return Service.DataManager.GetFile<TexFile>(path);
    }

    private void DrawMapMarkers() {
        DrawStaticMapMarkers();
        DrawDynamicMarkers();
        DrawGameObjects();
        DrawGroupMembers();
        DrawTemporaryMarkers();
        DrawGatheringMarkers();
        DrawFieldMarkers();
        DrawPlayer();
        DrawStaticTextMarkers();
        DrawFlag();
    }
}
