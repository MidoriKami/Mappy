using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Dalamud.Game.ClientState.Fates;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using KamiLib.AutomaticUserInterface;
using KamiLib.Caching;
using KamiLib.Utilities;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.ContextMenu;
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

[Category("DirectionalMarker", 1)]
public interface IFateDistanceMarkerConfig
{
    [BoolConfig("DirectionalMarker")]
    public bool EnableDirectionalMarker { get; set; }
    
    [FloatConfig("DistanceThreshold", 0.0f, 50.0f)]
    public float DistanceThreshold { get; set; }
}

[Category("ModuleConfig")]
public class FateConfig : IModuleConfig, IIconConfig, ITooltipConfig, IFateColorsConfig, IFateDistanceMarkerConfig
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

    public bool EnableDirectionalMarker { get; set; } = true;
    public float DistanceThreshold { get; set; } = 20.0f;
}

public unsafe class Fates : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.FATEs;
    public override IModuleConfig Configuration { get; protected set; } = new FateConfig();

    protected override bool ShouldDrawMarkers(Map map)
    {
        if (Service.ClientState.TerritoryType != map.TerritoryType.Row) return false;
        if (ParentMapContextMenuEntry.GetParentMap() is not null) return false;
        
        return base.ShouldDrawMarkers(map);
    }

    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        var config = GetConfig<FateConfig>();
        
        foreach (var fate in FateManager.Instance()->Fates.Span)
        {
            if (fate.Value is null) continue;
            
            DrawUtilities.DrawMapIcon(new MappyMapIcon
            {
                IconId = fate.Value->MapIconId,
                ObjectPosition = new Vector2(fate.Value->Location.X, fate.Value->Location.Z),
                IconScale = config.IconScale,
                ShowIcon = config.ShowIcon,
            
                Radius = fate.Value->Radius,
                RadiusColor = GetFateRingColor(fate),
            
                Tooltip = GetFatePrimaryTooltip(fate),
                TooltipDescription = GetFateSecondaryTooltip(fate, fate.Value->IsExpBonus),
                ShowTooltip = config.ShowTooltip,
                TooltipColor = config.TooltipColor,
            
                Layers = GetFateLayers(fate),
                VerticalPosition = fate.Value->Location.Y,
                ShowDirectionalIndicator = config.EnableDirectionalMarker,
                VerticalThreshold = config.DistanceThreshold,
            }, viewport, map);
        }
    }

    private List<IconLayer> GetFateLayers(FateContext* fate) => fate->IsExpBonus ? new List<IconLayer> { new(60934, new Vector2(16.0f, -16.0f)) } : new List<IconLayer>();

    private Vector4 GetFateRingColor(FateContext* fate)
    {
        var config = GetConfig<FateConfig>();
        
        var timeRemaining = GetTimeRemaining(fate);
        var earlyWarningTime = TimeSpan.FromSeconds(config.EarlyWarningTime);
        
        if (config.ExpiringWarning && timeRemaining <= earlyWarningTime)
        {
            return config.ExpiringColor;
        }

        return config.CircleColor;
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

    private static string GetTimeFormatted(TimeSpan span) => $"{span.Minutes:D2}:{span.Seconds:D2}";
}