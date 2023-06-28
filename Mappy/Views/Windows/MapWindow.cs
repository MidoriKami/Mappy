using System.Numerics;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using KamiLib.Atk;
using KamiLib.Commands;
using KamiLib.GameState;
using Mappy.Models;
using Mappy.System;
using Mappy.Utility;
using Mappy.Views.Components;

namespace Mappy.Views.Windows;

public unsafe class MapWindow : Window
{
    private static AtkUnitBase* NameplateAddon => (AtkUnitBase*) Service.GameGui.GetAddonByName("NamePlate");
    
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
        if (MappySystem.SystemConfig.HideWithGameGui && Node.IsAddonReady(NameplateAddon) && !NameplateAddon->RootNode->IsVisible) return false;
        
        return true;
    }

    public override void OnOpen()
    {
        UIModule.PlaySound(23u, 0, 0, 0);
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
            var unfocusedFade = MappySystem.SystemConfig.FadeWhenUnfocused && !IsFocused;
            var movementFade = MappySystem.SystemConfig.FadeWhenMoving && AgentMap.Instance()->IsPlayerMoving == 1;
            
            var fadePercent = unfocusedFade || movementFade ? 1.0f - MappySystem.SystemConfig.FadePercent : 1.0f;
            ImGui.Image(texture.ImGuiHandle, scaledSize, Vector2.Zero, Vector2.One, Vector4.One with { W = fadePercent });

            if (!toolbar.MapSelect.ShowMapSelectOverlay) MappySystem.ModuleController.Draw(Viewport, map);

            toolbar.Draw();
            MappySystem.ContextMenuController.Draw();
        }
        ImGui.EndChild();
    }

    public override void OnClose()
    {
        UIModule.PlaySound(24u, 0, 0, 0);
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

    public static bool IsCursorInWindow()
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

    [DoubleTierCommandHandler("GoToCommandHelp", "map", "goto")]
    private void GoToCommand(params string[] args)
    {
        if (args.Length is not 2) return;
        
        var x = float.Parse(args[0]);
        var y = float.Parse(args[1]);

        if (MappySystem.MapTextureController is { Ready: true, CurrentMap: var map })
        {
            var worldX = ConvertMapToWorld(x, map.SizeFactor, map.OffsetX);
            var worldY = ConvertMapToWorld(y, map.SizeFactor, map.OffsetY);

            MappySystem.SystemConfig.FollowPlayer = false;
            IsOpen = true;
                
            Viewport.SetViewportCenter(new Vector2(worldX, worldY));
            Viewport.SetViewportZoom(2.0f);
            
            // todo: add custom flag marker where focus becomes
            //
            // PluginLog.Debug(Utility.Position.GetObjectPosition(Viewport.Center, map).ToString());
            //
            // GatheringArea.SetGatheringAreaMarker(new TemporaryMapMarker
            // {
            //     Position = Utility.Position.GetObjectPosition(Viewport.Center, map) - new Vector2(2048.0f),
            //     TooltipText = "Goto Command",
            //     IconID = 60561, // Flag Marker
            //     Radius = 50.0f,
            //     Type = MarkerType.Flag,
            //     MapID = map.RowId
            // });
        }
    }
    
    private static float ConvertMapToWorld(float value, uint scale, int offset)
    {
        var scaleFactor = scale / 100.0f;
       
        return - offset * scaleFactor + 50.0f * (value - 1) * scaleFactor;
    }
}