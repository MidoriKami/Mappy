using Dalamud.Game.ClientState;
using Dalamud.IoC;
using Dalamud.Plugin;

namespace Mappy;

public sealed class Service
{
    [PluginService] public static DalamudPluginInterface PluginInterface { get; set; } = null!;
    [PluginService] public static ClientState ClientState { get; set; } = null!;
}