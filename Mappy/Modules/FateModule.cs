using System;
using System.Drawing;
using Dalamud.Interface;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using Lumina.Text.ReadOnly;
using Mappy.Classes;

namespace Mappy.Modules;

public class FateModule : ModuleBase {
    public override unsafe bool ProcessMarker(MarkerInfo markerInfo) {
        var markerName = markerInfo.PrimaryText?.Invoke();
        if (markerName.IsNullOrEmpty()) return false;
        
        foreach (var fate in Service.FateTable) {
            var name = new ReadOnlySeStringSpan(((FateContext*) fate.Address)->Name.AsSpan()).ExtractText();
            
            if (name.Equals(markerName, StringComparison.OrdinalIgnoreCase)) {
                var timeRemaining = TimeSpan.FromSeconds(fate.TimeRemaining);
                
                markerInfo.PrimaryText = () => $"Lv. {fate.Level} {fate.Name}";

                if (timeRemaining >= TimeSpan.Zero) {
                    markerInfo.SecondaryText = () => $"Time Remaining {timeRemaining:mm\\:ss}\nProgress {fate.Progress}%";

                    markerInfo.RadiusColor = timeRemaining.TotalSeconds switch {
                        < 60 => KnownColor.Red.Vector(),
                        < 120 => KnownColor.OrangeRed.Vector(),
                        < 180 => KnownColor.Orange.Vector(),
                        < 240 => KnownColor.Yellow.Vector(),
                        < 300 => KnownColor.YellowGreen.Vector(),
                        _ => markerInfo.RadiusColor
                    };
                }
                else {
                    markerInfo.SecondaryText = () => $"Progress {fate.Progress}%";
                }
                
                return true;
            }
        }

        return false;
    }
}