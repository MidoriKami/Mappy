using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Fates;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using KamiLib.Game;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Models.ModuleConfiguration;
using Mappy.System.Localization;

namespace Mappy.System.Modules;

public unsafe class Fates : ModuleBase {
    public override ModuleName ModuleName => ModuleName.FATEs;
    public override IModuleConfig Configuration { get; protected set; } = new FateConfig();

    protected override bool ShouldDrawMarkers(Map map) {
        if (Service.ClientState.TerritoryType != map.TerritoryType.Row) return false;
        
        return base.ShouldDrawMarkers(map);
    }

    protected override void UpdateMarkers(Viewport viewport, Map map) {
        foreach (FateContext* fate in FateManager.Instance()->Fates.Span) {
            if (fate is null) continue;
            
            UpdateIcon(fate->FateId,() => new MappyMapIcon {
                MarkerId = fate->FateId,
                IconId = fate->MapIconId,
                ObjectPosition = new Vector2(fate->Location.X, fate->Location.Z),
                MinimumRadius = (FateState)fate->State is FateState.Running ? fate->Radius : 0.0f,
                RadiusColor = GetFateRingColor(fate),
                Tooltip = $"Lv. {fate->Level} {fate->Name}",
                TooltipExtraText = GetFateSecondaryTooltip(fate, fate->IsExpBonus),
                Layers = GetFateLayers(fate),
                VerticalPosition = fate->Location.Y,
            }, icon => {
                icon.IconId = fate->MapIconId;
                icon.ObjectPosition = new Vector2(fate->Location.X, fate->Location.Z);
                icon.MinimumRadius = (FateState) fate->State is FateState.Running ? fate->Radius : 0.0f;
                icon.RadiusColor = GetFateRingColor(fate);
                icon.TooltipExtraText = GetFateSecondaryTooltip(fate, fate->IsExpBonus);
                icon.VerticalPosition = fate->Location.Y;
            });
        }
    }

    private List<IconLayer> GetFateLayers(FateContext* fate) 
        => fate->IsExpBonus ? 
            new List<IconLayer> { new(60934, new Vector2(16.0f, -16.0f)) } : 
            new List<IconLayer>();

    private Vector4 GetFateRingColor(FateContext* fate) {
        var config = GetConfig<FateConfig>();
        
        var timeRemaining = GetTimeRemaining(fate);
        var earlyWarningTime = TimeSpan.FromSeconds(config.EarlyWarningTime);
        
        if (config.ExpiringWarning && timeRemaining <= earlyWarningTime) {
            return config.ExpiringColor;
        }

        return config.CircleColor;
    }
    
    private string GetFateSecondaryTooltip(FateContext* fate, bool isExpBonus) {
        if ((FateState) fate->State != FateState.Running) return isExpBonus ? LuminaCache<Addon>.Instance.GetRow(3921)!.Text.RawString : string.Empty;

        var baseString = $"{Strings.TimeRemaining}: {GetTimeFormatted(GetTimeRemaining(fate))}\n{Strings.Progress}: {fate->Progress,3}%%";

        if (isExpBonus) {
            baseString += $"\n{LuminaCache<Addon>.Instance.GetRow(3921)!.Text.RawString}";
        }

        return baseString;
    }

    private TimeSpan GetTimeRemaining(FateContext* fate) {
        var now = DateTime.UtcNow;
        var start = DateTimeOffset.FromUnixTimeSeconds(fate->StartTimeEpoch).UtcDateTime;
        var duration = TimeSpan.FromSeconds(fate->Duration);

        var delta = duration - (now - start);

        return delta;
    }

    private static string GetTimeFormatted(TimeSpan span) => $"{span.Minutes:D2}:{span.Seconds:D2}";
}