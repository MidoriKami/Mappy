using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.Toast;
using Dalamud.IoC;
using Dalamud.Plugin;

namespace Mappy;

public sealed class Service
{
    [PluginService] public static DalamudPluginInterface PluginInterface { get; set; } = null!;
    [PluginService] public static ClientState ClientState { get; set; } = null!;
    [PluginService] public static Framework Framework { get; set; } = null!;
    [PluginService] public static DataManager DataManager { get; set; } = null!;
    [PluginService] public static ChatGui Chat { get; set; } = null!;
    [PluginService] public static ToastGui Toast { get; set; } = null!;
}