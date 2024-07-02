using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Ipc.Exceptions;
using Lumina.Excel.GeneratedSheets;

namespace Mappy.Classes;

public class Teleporter {
    private readonly ICallGateSubscriber<uint, byte, bool> teleportIpc = Service.PluginInterface.GetIpcSubscriber<uint, byte, bool>("Teleport");
    private readonly ICallGateSubscriber<bool> showChatMessageIpc = Service.PluginInterface.GetIpcSubscriber<bool>("Teleport.ChatMessage");

    public void Teleport(Aetheryte aetheryte) {
        try {
            var didTeleport = teleportIpc.InvokeFunc(aetheryte.RowId, (byte) aetheryte.SubRowId);
            var showMessage = showChatMessageIpc.InvokeFunc();

            if (!didTeleport) {
                UserError("Cannot teleport in this situation.");
            }
            else if (showMessage) {
                Service.ChatGui.Print(new XivChatEntry {
                    Message = new SeStringBuilder()
                        .AddUiForeground("[Mappy] ", 45)
                        .AddUiForeground($"[Teleport] ", 62)
                        .AddText($"Teleporting to ")
                        .AddUiForeground(aetheryte.PlaceName.Value?.Name ?? "Unable to read name", 576)
                        .Build(),
                });
            }
        } catch (IpcNotReadyError) {
            Service.Log.Error("Teleport IPC not found");
            UserError("To use the teleport function, you must install the 'Teleporter' plugin");
        }
    }

    private static void UserError(string error) {
        Service.ChatGui.PrintError(error);
        Service.ToastGui.ShowError(error);
    }
}