using System;
using System.Drawing;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using KamiLib;
using KamiLib.AutomaticUserInterface;
using KamiLib.Caching;
using KamiLib.Utilities;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Utility;
using Mappy.Views.Windows;

namespace Mappy.System.Modules;

public class PlayerConfig : IconModuleConfigBase
{
    [Disabled]
    public new bool ShowTooltip = false;
    
    [BoolConfigOption("ShowCone", "ModuleConfig", 0)]
    public bool ShowCone = true;

    [FloatConfigOption("ConeRadius", "ModuleConfig", 0, 0.0f, 360.0f)]
    public float ConeRadius = 90.0f;

    [FloatConfigOption("ConeAngle", "ModuleConfig", 0, 0.0f, 180.0f)]
    public float ConeAngle = 90.0f;

    [FloatConfigOption("OutlineThickness", "ModuleConfig", 0, 0.5f, 5.0f)]
    public float OutlineThickness = 2.0f;

    [ColorConfigOption("OutlineColor", "ColorOptions", 1, 128, 128, 128, 255)]
    public Vector4 OutlineColor = KnownColor.Gray.AsVector4();
    
    [ColorConfigOption("FillColor", "ColorOptions", 1, 173, 216, 230, 45)]
    public Vector4 FillColor = KnownColor.LightBlue.AsVector4() with { W = 45 };
}

public unsafe class Player : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.Player;
    public override ModuleConfigBase Configuration { get; protected set; } = new PlayerConfig();
        
    public override void LoadForMap(MapData mapData)
    {
        // Do Nothing.
    }
    
    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        var config = GetConfig<PlayerConfig>();
        
        if (!config.Enable) return;
        if (AgentMap.Instance()->CurrentMapId != map.RowId) return;
        if (Service.ClientState.LocalPlayer is not { } player) return;
        
        if(config.ShowCone) DrawLookLine(player);
        if(config.ShowIcon) DrawBluePlayerIcon(player);
    }
    
    private void DrawBluePlayerIcon(GameObject player)
    {
        var config = GetConfig<PlayerConfig>();
        
        var icon = IconCache.Instance.GetIcon(60443); 
        DrawUtilities.DrawImageRotated(icon, player, config.IconScale);
    }

    private void DrawLookLine(GameObject player)
    {
        var config = GetConfig<PlayerConfig>();
        if (MappySystem.MapTextureController is not { Ready: true, CurrentMap: var map }) return;
        if (KamiCommon.WindowManager.GetWindowOfType<MapWindow>() is not { } mapWindow) return;
        
        var angle = GetCameraRotation();

        var playerPosition = Position.GetObjectPosition(player, map);
        var drawPosition = mapWindow.Viewport.GetImGuiWindowDrawPosition(playerPosition);

        var lineLength = config.ConeRadius * mapWindow.Viewport.Scale;
        
        var halfConeAngle = DegreesToRadians(config.ConeAngle) / 2.0f;
        
        DrawAngledLineFromCenter(drawPosition, lineLength, angle - halfConeAngle);
        DrawAngledLineFromCenter(drawPosition, lineLength, angle + halfConeAngle);
        DrawLineArcFromCenter(drawPosition, lineLength, angle);
        
        DrawFilledSemiCircle(drawPosition, lineLength, angle);
    }

    private void DrawAngledLineFromCenter(Vector2 center, float lineLength, float angle)
    {
        var config = GetConfig<PlayerConfig>();
        
        var lineSegment = new Vector2(lineLength * MathF.Cos(angle), lineLength * MathF.Sin(angle));
        ImGui.GetWindowDrawList().AddLine(center, center + lineSegment, ImGui.GetColorU32(config.OutlineColor), config.OutlineThickness);
    }

    private void DrawLineArcFromCenter(Vector2 center, float distance, float rotation)
    {
        var config = GetConfig<PlayerConfig>();
        var halfConeAngle = DegreesToRadians(config.ConeAngle) / 2.0f;
        
        var start = rotation - halfConeAngle;
        var stop = rotation + halfConeAngle;
        
        ImGui.GetWindowDrawList().PathArcTo(center, distance, start, stop);
        ImGui.GetWindowDrawList().PathStroke(ImGui.GetColorU32(config.OutlineColor), ImDrawFlags.None, config.OutlineThickness);
    }

    private void DrawFilledSemiCircle(Vector2 center, float distance, float rotation)
    {
        var config = GetConfig<PlayerConfig>();
        var halfConeAngle = DegreesToRadians(config.ConeAngle) / 2.0f;
        
        var startAngle = rotation - halfConeAngle;
        var stopAngle = rotation + halfConeAngle;
        
        var startPosition = new Vector2(distance * MathF.Cos(rotation - halfConeAngle), distance * MathF.Sin(rotation - halfConeAngle));

        ImGui.GetWindowDrawList().PathArcTo(center, distance, startAngle, stopAngle);
        ImGui.GetWindowDrawList().PathLineTo(center);
        ImGui.GetWindowDrawList().PathLineTo(center + startPosition);
        ImGui.GetWindowDrawList().PathFillConvex(ImGui.GetColorU32(config.FillColor));
    }
    
    private float GetCameraRotation() => -DegreesToRadians(AtkStage.GetSingleton()->GetNumberArrayData()[24]->IntArray[3]) - 0.5f * MathF.PI;

    private float DegreesToRadians(float degrees) => MathF.PI / 180.0f * degrees;
}