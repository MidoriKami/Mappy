using System;
using System.Drawing;
using System.Numerics;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using FFXIVClientStructs.Interop;

namespace Mappy.Extensions;

public static unsafe class FateContextExtensions {
    public static Vector4 GetColor(this Pointer<FateContext> context, float alpha = 0.33f) {
        var timeRemaining = GetTimeRemaining(context);
        if (timeRemaining <= TimeSpan.FromSeconds(300) && timeRemaining.TotalSeconds > 0) {
            var hue = (float)(timeRemaining.TotalSeconds / 300.0f * 25.0f);

            var hsvColor = new ColorHelpers.HsvaColor(hue / 100.0f, 1.0f, 1.0f, alpha);
            return ColorHelpers.HsvToRgb(hsvColor);
        }

        return KnownColor.White.Vector();
    }

    public static TimeSpan GetTimeRemaining(this Pointer<FateContext> context) {
        if (context.Value->Duration is 0) return TimeSpan.Zero;
        
        return TimeSpan.FromSeconds(context.Value->StartTimeEpoch + context.Value->Duration - DateTimeOffset.Now.ToUnixTimeSeconds());
    }
}