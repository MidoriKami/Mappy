using System;
using System.Drawing;
using System.Numerics;
using Dalamud.Game.ClientState.Fates;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using ImGuiNET;
using KamiLib.AutomaticUserInterface;
using KamiLib.Utilities;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Utility;

namespace Mappy.System.Modules;

public class FateConfig : IconModuleConfigBase
{
    [BoolConfigOption("ShowRing", "ModuleConfig", 0)]
    public bool ShowRing = true;
    
    [BoolConfigOption("ExpiringWarning", "ModuleConfig", 0)]
    public bool ExpiringWarning = false;

    [IntCounterConfigOption("EarlyWarningTime", "ModuleConfig", 0, false)]
    public int EarlyWarningTime = 300;
    
    [ColorConfigOption("TooltipColor", "ModuleColors", 1, 1.0f, 1.0f, 1.0f, 1.0f)]
    public Vector4 TooltipColor = KnownColor.White.AsVector4();
    
    [ColorConfigOption("CircleColor", "ModuleColors", 1, 0.58f, 0.388f, 0.827f, 0.33f)]
    public Vector4 CircleColor = new(0.58f, 0.388f, 0.827f, 0.33f);

    [ColorConfigOption("ExpiringColor", "ModuleColors", 1, 1.0f, 0.0f, 0.0f, 0.33f)]
    public Vector4 ExpiringColor = KnownColor.Red.AsVector4() with { W = 0.33f };
}

public unsafe class Fates : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.FATEs;
    public override ModuleConfigBase Configuration { get; protected set; } = new FateConfig();

    protected override bool ShouldDrawMarkers(Map map)
    {
        if (!IsPlayerInCurrentMap(map)) return false;
        
        return base.ShouldDrawMarkers(map);
    }

    public override void LoadForMap(MapData mapData)
    {
        // Do Nothing.
    }
    
    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        foreach (var fate in FateManager.Instance()->Fates.Span)
        {
            if (fate.Value is null) continue;
            DrawFate(fate, viewport, map);
        }
    }
    
    private void DrawFate(FateContext* fate, Viewport viewport, Map map)
    {
        var config = GetConfig<FateConfig>();
        var position = Position.GetObjectPosition(fate->Location, map);

        if (config.ShowRing) DrawRing(fate, viewport, map);
        if (config.ShowIcon) DrawUtilities.DrawIcon(fate->IconId, position, config.IconScale);
        if (config.ShowTooltip) DrawTooltip(fate);
    }

    private void DrawRing(FateContext* fate, Viewport viewport, Map map)
    {
        var config = GetConfig<FateConfig>();
        
        var timeRemaining = GetTimeRemaining(fate);
        var earlyWarningTime = TimeSpan.FromSeconds(config.EarlyWarningTime);
        var color = ImGui.GetColorU32(config.CircleColor);

        if (config.ExpiringWarning && timeRemaining <= earlyWarningTime)
        {
            color = ImGui.GetColorU32(config.ExpiringColor);
        }

        switch ((FateState)fate->State)
        {
            case FateState.Running:
                var position = Position.GetObjectPosition(fate->Location, map);
                var drawPosition = viewport.GetImGuiWindowDrawPosition(position);

                var radius = fate->Radius * viewport.Scale;

                ImGui.GetWindowDrawList().AddCircleFilled(drawPosition, radius, color);
                ImGui.GetWindowDrawList().AddCircle(drawPosition, radius, color, 0, 4);
                break;
        }
    }

    private void DrawTooltip(FateContext* fate)
    {
        if (!ImGui.IsItemHovered()) return;
        var config = GetConfig<FateConfig>();

        ImGui.BeginTooltip();

        switch ((FateState)fate->State)
        {
            case FateState.Running:
                var remainingTime = GetTimeFormatted(GetTimeRemaining(fate));

                ImGui.TextColored(config.TooltipColor,
                    $"Level {fate->Level} {fate->Name}\n" +
                    $"Time Remaining: {remainingTime}\n" +
                    $"Progress: {fate->Progress}%%");
                break;

            case FateState.Preparation:
                ImGui.TextColored(config.TooltipColor,
                    $"Level {fate->Level} {fate->Name}");
                break;
        }

        ImGui.EndTooltip();
    }

    private TimeSpan GetTimeRemaining(FateContext* fate)
    {
        var now = DateTime.UtcNow;
        var start = DateTimeOffset.FromUnixTimeSeconds(fate->StartTimeEpoch).UtcDateTime;
        var duration = TimeSpan.FromSeconds(fate->Duration);

        var delta = duration - (now - start);

        return delta;
    }

    private string GetTimeFormatted(TimeSpan span) => $"{span.Minutes:D2}:{span.Seconds:D2}";
}