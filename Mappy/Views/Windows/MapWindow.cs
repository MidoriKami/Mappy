using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using KamiLib.Commands;
using KamiLib.GameState;
using Mappy.Models;
using Mappy.System;
using Mappy.Utility;
using Mappy.Views.Components;

namespace Mappy.Views.Windows;

public class MapWindow : Window
{
    public Viewport Viewport = new();
    private Vector2 mouseDragStart;
    private bool dragStarted;
    private Vector2 lastWindowSize = Vector2.Zero;
    private readonly MapToolbar toolbar;
    
    public Vector2 MapContentsStart { get; private set; }

    private const ImGuiWindowFlags DefaultFlags = ImGuiWindowFlags.NoFocusOnAppearing |
                                                  ImGuiWindowFlags.NoNav |
                                                  ImGuiWindowFlags.NoBringToFrontOnFocus |
                                                  ImGuiWindowFlags.NoScrollbar |
                                                  ImGuiWindowFlags.NoScrollWithMouse |
                                                  ImGuiWindowFlags.NoDocking;

    public const ImGuiWindowFlags NoDecorationFlags =
        ImGuiWindowFlags.NoDecoration |
        ImGuiWindowFlags.NoBackground;

    public const ImGuiWindowFlags NoMoveResizeFlags =
        ImGuiWindowFlags.NoMove |
        ImGuiWindowFlags.NoResize;
    
    public MapWindow() : base("Mappy - Map Window")
    {
        toolbar = new MapToolbar(this);
        
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(470,200),
            MaximumSize = new Vector2(9999,9999)
        };

        CommandController.RegisterCommands(this);
    }

    public override void PreOpenCheck()
    {
        if (MappySystem.SystemConfig.KeepOpen) IsOpen = true;
        if (Service.ClientState.IsPvP) IsOpen = false;
        if (!Service.ClientState.IsLoggedIn) IsOpen = false;
    }
    
    public override bool DrawConditions()
    {
        if (!Service.ClientState.IsLoggedIn) return false;
        if (Service.ClientState.IsPvP) return false;
        if (MappySystem.SystemConfig.HideInDuties && Condition.IsBoundByDuty()) return false;
        if (MappySystem.SystemConfig.HideInCombat && Condition.IsInCombat()) return false;
        if (MappySystem.SystemConfig.HideBetweenAreas && Condition.IsBetweenAreas()) return false;
        
        return true;
    }
    
    public override void Draw()
    {
        if (MappySystem.MapTextureController is not { Ready: true, MapTexture: var texture, CurrentMap: var map }) return;
        
        SetWindowFlags();
        ReadMouseInputs();
        
        MapContentsStart = ImGui.GetCursorScreenPos();
        if (ImGui.BeginChild("###MapFrame", ImGui.GetContentRegionAvail(), false, Flags))
        {
            Viewport.UpdateSize();
            var scaledSize = new Vector2(texture.Width, texture.Height) * Viewport.Scale;
            
            Viewport.SetImGuiDrawPosition();
            var fadePercent = MappySystem.SystemConfig.FadeWhenUnfocused && !IsFocused ? 1.0f - MappySystem.SystemConfig.FadePercent : 1.0f;
            ImGui.Image(texture.ImGuiHandle, scaledSize, Vector2.Zero, Vector2.One, Vector4.One with { W = fadePercent });

            if (!toolbar.MapSelect.ShowMapSelectOverlay) MappySystem.ModuleController.Draw(Viewport, map);

            toolbar.Draw();
            MappySystem.ContextMenuController.Draw();
        }
        ImGui.EndChild();
    }
    
    private void SetWindowFlags()
    {
        Flags = DefaultFlags;
        
        if (MappySystem.SystemConfig.LockWindow) Flags |= NoMoveResizeFlags;
        if (MappySystem.SystemConfig.HideWindowFrame) Flags |= NoDecorationFlags;
        if (!IsCursorInWindowHeader()) Flags |= ImGuiWindowFlags.NoMove;
    }

    private void ReadMouseInputs()
    {
        if (!ImGui.IsWindowFocused(ImGuiFocusedFlags.AnyWindow) || IsFocused)
        {
            // Only allow Context, Zoom, an DragStart if cursor is over the map
            if (IsCursorInWindow() && !IsCursorInWindowHeader())
            {
                ProcessContextMenu();
                ProcessZoomChange();
                ProcessMapDragStart();
            }

            // Allow DragStop to be outside the map window
            ProcessMapDragStop();
        }
    }
    
    private void ProcessMapDragStop()
    {
        if (ImGui.IsMouseDragging(ImGuiMouseButton.Left) && dragStarted)
        {
            var delta = mouseDragStart - ImGui.GetMousePos();
            Viewport.MoveViewportCenter(delta);
            mouseDragStart = ImGui.GetMousePos();
        }
        else
        {
            dragStarted = false;
        }

        if (dragStarted)
        {
            MappySystem.SystemConfig.FollowPlayer = false;
        }
    }

    private void ProcessMapDragStart()
    {
        // Don't allow a drag to start if the window size is changing
        if (ImGui.GetWindowSize() == lastWindowSize)
        {
            if (ImGui.IsMouseDown(ImGuiMouseButton.Left) && !dragStarted)
            {
                mouseDragStart = ImGui.GetMousePos();
                dragStarted = true;
            }
        }
        else
        {
            lastWindowSize = ImGui.GetWindowSize();
            dragStarted = false;
        }
    }
    
    private void ProcessContextMenu()
    {
        if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
        {
            MappySystem.ContextMenuController.Show(ContextMenuType.General);
        }
    }
    
    private void ProcessZoomChange()
    {
        if (ImGui.GetIO().MouseWheel > 0) // Mouse Wheel Up
        {
            Viewport.ZoomIn(MappySystem.SystemConfig.ZoomSpeed);
        }
        else if (ImGui.GetIO().MouseWheel < 0) // Mouse Wheel Down
        {
            Viewport.ZoomOut(MappySystem.SystemConfig.ZoomSpeed);
        }
    }

    private static bool IsCursorInWindow()
    {
        var windowStart = ImGui.GetWindowPos();
        var windowSize = ImGui.GetWindowSize();

        return Bound.IsBoundedBy(ImGui.GetMousePos(), windowStart, windowStart + windowSize);
    }
    
    private static bool IsCursorInWindowHeader()
    {
        var windowStart = ImGui.GetWindowPos();
        var headerSize = ImGui.GetWindowSize() with { Y = ImGui.GetWindowContentRegionMin().Y };
        
        return Bound.IsBoundedBy(ImGui.GetMousePos(), windowStart, windowStart + headerSize);
    }
}