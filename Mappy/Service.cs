using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace Mappy;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
public class Service {
    [PluginService] public static IDalamudPluginInterface PluginInterface { get; set; }
    [PluginService] public static IClientState ClientState { get; set; }
    [PluginService] public static IDataManager DataManager { get; set; }
    [PluginService] public static ITextureProvider TextureProvider { get; set; }
    [PluginService] public static IObjectTable ObjectTable { get; set; }
    [PluginService] public static IGameGui GameGui { get; set; }
    [PluginService] public static IAetheryteList AetheryteList { get; set; }
    [PluginService] public static IPluginLog Log { get; set; }
    [PluginService] public static IGameInteropProvider Hooker { get; set; }
    [PluginService] public static IFateTable FateTable { get; set; }
    [PluginService] public static ICondition Condition { get; set; }
    [PluginService] public static IKeyState KeyState { get; set; }
    [PluginService] public static ITextureSubstitutionProvider TextureSubstitutionProvider { get; set; }
    [PluginService] public static IFramework Framework { get; set; }
    [PluginService] public static IAddonLifecycle AddonLifecycle { get; set; }
}