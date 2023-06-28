using System;
using KamiLib.AutomaticUserInterface;

namespace Mappy.Models.Enums;

[Flags]
public enum FadeMode
{
    [EnumLabel("Always")]
    Always = 0x01,
    
    [EnumLabel("WhenMoving")]
    WhenMoving = 0x02,
    
    [EnumLabel("WhenFocused")]
    WhenFocused = 0x04,
    
    [EnumLabel("WhenUnFocused")]
    WhenUnFocused = 0x08,
}