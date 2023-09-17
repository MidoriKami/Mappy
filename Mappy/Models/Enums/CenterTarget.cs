using KamiLib.AutomaticUserInterface;
using System;

namespace Mappy.Models.Enums;

public enum CenterTarget
{
    [EnumLabel("Disabled")]
    Disabled = 0,

    [EnumLabel("Player")]
    Player = 1,

    [EnumLabel("Map")]
    Map = 2
}
