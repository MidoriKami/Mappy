using System;
using System.Drawing;
using System.Numerics;
using Dalamud.Game.ClientState.Fates;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using ImGuiNET;
using KamiLib.AutomaticUserInterface;
using KamiLib.Caching;
using KamiLib.Utilities;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.System.Localization;
using Mappy.Utility;

namespace Mappy.System.Modules;

[Category("ModuleColors")]
public interface IFateColorsConfig
{
    [ColorConfig("CircleColor", 0.58f, 0.388f, 0.827f, 0.33f)]
    public Vector4 CircleColor { get; set; }
    
    [ColorConfig("ExpiringColor", 1.0f, 0.0f, 0.0f, 0.33f)]
    public Vector4 ExpiringColor { get; set; }
}

[Category("ModuleConfig")]
public class FateConfig : IModuleConfig, IIconConfig, ITooltipConfig, IFateColorsConfig
{
    public bool Enable { get; set; } = true;
    public int Layer { get; set; } = 3;
    public bool ShowIcon { get; set; } = true;
    public float IconScale { get; set; } = 0.50f;
    public bool ShowTooltip { get; set; } = true;
    public Vector4 TooltipColor { get; set; } = KnownColor.White.AsVector4();
    public Vector4 CircleColor { get; set; } = new(0.58f, 0.388f, 0.827f, 0.33f);
    public Vector4 ExpiringColor { get; set; } = KnownColor.Red.AsVector4() with { W = 0.33f };
    
    [BoolConfig("ShowRing")]
    public bool ShowRing { get; set; } = true;
    
    [BoolConfig("ExpiringWarning")]
    public bool ExpiringWarning { get; set; } = false;

    [IntCounterConfig("EarlyWarningTime", false)]
    public int EarlyWarningTime { get; set; } = 300;
}

public unsafe class Fates : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.FATEs;
    public override IModuleConfig Configuration { get; protected set; } = new FateConfig();

    protected override bool ShouldDrawMarkers(Map map)
    {
        if (!IsPlayerInCurrentMap(map)) return false;
        
        return base.ShouldDrawMarkers(map);
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

        var mapIcon = *(uint*) ((byte*) fate + 0x760);
        var isExpBonus = *(bool*) ((byte*) fate + 0x3C4);
        var primaryLine = GetFatePrimaryTooltip(fate);
        var secondaryLine = GetFateSecondaryTooltip(fate, isExpBonus);
        
        if (config.ShowRing) DrawRing(fate, viewport, map);
        if (config.ShowIcon) DrawUtilities.DrawIcon(mapIcon, position, config.IconScale);
        if (config.ShowTooltip && (fate->State is 7 || viewport.Scale <= 0.5f)) DrawUtilities.DrawTooltip(mapIcon, config.TooltipColor, primaryLine, secondaryLine);
        if (config.ShowTooltip && (fate->State is not 7 && viewport.Scale > 0.5f)) DrawUtilities.DrawLevelTooltip(new Vector2(fate->Location.X, fate->Location.Z), fate->Radius * viewport.Scale, viewport, map, mapIcon, config.TooltipColor, primaryLine, secondaryLine);
        if (config.ShowIcon && isExpBonus) DrawUtilities.DrawIcon(60934, position + new Vector2(16.0f, -16.0f) * config.IconScale / viewport.Scale, config.IconScale);
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
    
    private string GetFatePrimaryTooltip(FateContext* fate) => $"Lv. {fate->Level} {fate->Name}";
    private string GetFateSecondaryTooltip(FateContext* fate, bool isExpBonus)
    {
        if ((FateState) fate->State != FateState.Running) return isExpBonus ? LuminaCache<Addon>.Instance.GetRow(3921)!.Text.RawString : string.Empty;

        var baseString = $"{Strings.TimeRemaining}: {GetTimeFormatted(GetTimeRemaining(fate))}\n{Strings.Progress}: {fate->Progress,3}%%";

        if (isExpBonus)
        {
            baseString += $"\n{LuminaCache<Addon>.Instance.GetRow(3921)!.Text.RawString}";
        }

        return baseString;
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