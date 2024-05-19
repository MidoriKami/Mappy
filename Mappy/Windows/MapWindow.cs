using System.Linq;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using KamiLib.CommandManager;
using KamiLib.Window;

namespace Mappy.Windows;

public class MapWindow : Window {
    public Vector2 MapDrawOffset { get; private set; }
    public bool IsMapHovered { get; private set; }
    
    private bool isDragStarted;
    private Vector2 lastWindowSize;
    
    public MapWindow() : base("Mappy Map Window", new Vector2(410.0f, 250.0f)) {
        System.CommandManager.RegisterCommand(new CommandHandler {
            ActivationPath = "/togglemap",
            Delegate = _ => System.MapWindow.UnCollapseOrToggle(),
        });
        
        System.CommandManager.RegisterCommand(new CommandHandler {
            ActivationPath = "/showmap",
            Delegate = _ => System.MapWindow.UnCollapseOrShow(),
        });
        
        System.CommandManager.RegisterCommand(new CommandHandler {
            ActivationPath = "/hidemap",
            Delegate = _ => System.MapWindow.Close(),
        });
        
        // Easter Egg, don't recommend executing this command.
        System.CommandManager.RegisterCommand(new CommandHandler {
            ActivationPath = "/pyon",
            Hidden = true,
            Delegate = _ => {
                foreach (var index in Enumerable.Range(0, 20)) {
                    Service.Framework.RunOnTick(Toggle, delayTicks: 20 * index);
                }
            }
        });
    }

    public override bool DrawConditions() {
        if (Service.ClientState is { IsLoggedIn: false } or { IsPvP: true }) return false;
        
        return true;
    }

    public override void PreOpenCheck() {
        // if (Service.SystemConfig.KeepOpen) IsOpen = true;
        if (Service.ClientState is { IsLoggedIn: false } or { IsPvP: true }) IsOpen = false;
    }
    
    public override void OnOpen() {
        System.IntegrationsController.TryYeetMap();
    }

    protected override void DrawContents() {
        MapDrawOffset = ImGui.GetCursorScreenPos();
        using (var renderChild = ImRaii.Child("render_child", ImGui.GetContentRegionAvail(), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar)) {
            if (renderChild) {
                System.MapRenderer.Draw();
            }
        }

        IsMapHovered = ImGui.IsItemHovered();
        
        if (IsMapHovered) {
            ProcessMouseScroll();
            ProcessMapDragStart();
            Flags |= ImGuiWindowFlags.NoMove;
        }
        else {
            Flags &= ~ImGuiWindowFlags.NoMove;
        }
        
        ProcessMapDragDragging();
        ProcessMapDragEnd();
    }

    public override void OnClose() {
        System.IntegrationsController.TryUnYeetMap();
    }

    private static void ProcessMouseScroll() {
        if (ImGui.GetIO().MouseWheel is 0) return;
        
        if (System.SystemConfig.UseLinearZoom) {
            System.MapRenderer.Scale += System.SystemConfig.ZoomSpeed * ImGui.GetIO().MouseWheel;
        }
        else {
            System.MapRenderer.Scale *= 1.0f + System.SystemConfig.ZoomSpeed * ImGui.GetIO().MouseWheel;
        }
    }
    
    private void ProcessMapDragDragging() {
        if (ImGui.IsMouseDragging(ImGuiMouseButton.Left) && isDragStarted) {
            System.MapRenderer.DrawOffset += ImGui.GetMouseDragDelta() / System.MapRenderer.Scale;
            ImGui.ResetMouseDragDelta();
        }
    }
    
    private void ProcessMapDragEnd() {
        if (ImGui.IsMouseReleased(ImGuiMouseButton.Left)) {
            isDragStarted = false;
        }
    }
    
    private void ProcessMapDragStart() {
        // Don't allow a drag to start if the window size is changing
        if (ImGui.GetWindowSize() == lastWindowSize) {
            if (ImGui.IsItemClicked(ImGuiMouseButton.Left) && !isDragStarted) {
                isDragStarted = true;
            }
        } else {
            lastWindowSize = ImGui.GetWindowSize();
            isDragStarted = false;
        }
    }
}