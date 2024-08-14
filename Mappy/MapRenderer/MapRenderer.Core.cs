using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;
using Mappy.Classes;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Utility;
using Lumina.Data.Files;

namespace Mappy.MapRenderer;

public partial class MapRenderer {
    public float Scale {
        get => System.SystemConfig.MapScale;
        set => System.SystemConfig.MapScale = value;
    }

    public Vector2 DrawOffset { get; set; }
    public Vector2 DrawPosition { get; private set; }

    private IDalamudTextureWrap? blendedTexture;
    private string blendedPath = string.Empty;

    public void CenterOnGameObject(IGameObject obj) 
        => DrawOffset = -new Vector2(obj.Position.X, obj.Position.Z) * DrawHelpers.GetMapScaleFactor() + DrawHelpers.GetMapOffsetVector();

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

    private unsafe IDalamudTextureWrap? LoadTexture() {
        var vanillaBgPath = $"{AgentMap.Instance()->SelectedMapBgPath.ToString()}.tex";
        var vanillaFgPath = $"{AgentMap.Instance()->SelectedMapPath.ToString()}.tex";
        
        var bgPath = Service.TextureSubstitutionProvider.GetSubstitutedPath(vanillaBgPath);
        var fgPath = Service.TextureSubstitutionProvider.GetSubstitutedPath(vanillaFgPath);
        
        TexFile? bgFile;
        TexFile? fgFile;

        var isRootedPath = Path.IsPathRooted(bgPath) || Path.IsPathRooted(fgPath);
        var isReplacedPathValid = new FileInfo(bgPath).Exists && new FileInfo(fgPath).Exists;
        
        if (isRootedPath && isReplacedPathValid) {
            bgFile = Service.DataManager.GameData.GetFileFromDisk<TexFile>(bgPath);
            fgFile = Service.DataManager.GameData.GetFileFromDisk<TexFile>(fgPath);
        }
        else {
            if (isRootedPath && !isReplacedPathValid) {
                Service.Log.Warning("Tried to load substituted path(s) but file was not found");
                Service.Log.Warning($"\t{vanillaBgPath} -> {bgPath}");
                Service.Log.Warning($"\t{vanillaFgPath} -> {fgPath}");
                Service.Log.Warning("Loading vanilla textures instead");
            }
            
            bgFile = Service.DataManager.GetFile<TexFile>($"{AgentMap.Instance()->SelectedMapBgPath.ToString()}.tex");
            fgFile = Service.DataManager.GetFile<TexFile>($"{AgentMap.Instance()->SelectedMapPath.ToString()}.tex"); 
        }

        if (bgFile is null || fgFile is null) {
            Service.Log.Warning("Failed to load map textures");
            return null;
        }

        var backgroundBytes = bgFile.GetRgbaImageData();
        var foregroundBytes = fgFile.GetRgbaImageData();

        Parallel.For(0, 2048 * 2048, i => {
            var index = i * 4;
            backgroundBytes[index + 0] = (byte)(backgroundBytes[index + 0] * foregroundBytes[index + 0] / 255);
            backgroundBytes[index + 1] = (byte)(backgroundBytes[index + 1] * foregroundBytes[index + 1] / 255);
            backgroundBytes[index + 2] = (byte)(backgroundBytes[index + 2] * foregroundBytes[index + 2] / 255);
        });

        return Service.TextureProvider.CreateFromRaw(RawImageSpecification.Rgba32(2048, 2048), backgroundBytes);
    }

    private void DrawMapMarkers() {
        DrawStaticMapMarkers();
        DrawHousingMarkers();
        DrawDynamicMarkers();
        DrawGameObjects();
        DrawGroupMembers();
        DrawTemporaryMarkers();
        DrawGatheringMarkers();
        DrawFieldMarkers();
        DrawPlayer();
        DrawFlag();
    }
}
