using System;
using KamiLib.AutomaticUserInterface;

namespace Mappy.Models.Enums;

[Flags]
public enum FadeMode {
    [EnumLabel("Always")]
    Always = 1 << 0,
    
    [EnumLabel("WhenMoving")]
    WhenMoving = 1 << 2,
    
    [EnumLabel("WhenFocused")]
    WhenFocused = 1 << 3,
    
    [EnumLabel("WhenUnFocused")]
    WhenUnFocused = 1 << 4,
}