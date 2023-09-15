using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using KamiLib;
using KamiLib.Commands;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.System;
using Mappy.System.Modules;
using Mappy.Utility;
using Mappy.Views.Components;
using Condition = KamiLib.GameState.Condition;
using Node = KamiLib.Atk.Node;

namespace Mappy.Views.Windows;

public unsafe class MapWindow : Window
{
    private static AtkUnitBase* NameplateAddon => (AtkUnitBase*)Service.GameGui.GetAddonByName("NamePlate");

    public Viewport Viewport = new();
    private Vector2 mouseDragStart;
    private bool dragStarted;
    private Vector2 lastWindowSize = Vector2.Zero;
    private readonly MapToolbar toolbar;
    public bool ProcessingCommand;

    public MapWindow() : base("Mappy - Map Window")
    {
        toolbar = new MapToolbar(this);

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(510, 200),
            MaximumSize = new Vector2(9999, 9999)
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

        if (!ProcessingCommand)
        {
            var centerTarget = MappySystem.SystemConfig.CenterOnOpen;

            if (centerTarget != CenterTarget.Disabled)
            {
                if (KamiCommon.WindowManager.GetWindowOfType<MapWindow>() is not { Viewport: var viewport }) return;
                if (Service.ClientState.LocalPlayer is not { Position: var playerPosition }) return;
                if (MappySystem.MapTextureController is not { Ready: true, CurrentMap: var map }) return;

                if (centerTarget == CenterTarget.Player)
                {
                    MappySystem.MapTextureController.MoveMapToPlayer();
                    viewport.SetViewportCenter(Utility.Position.GetTexturePosition(playerPosition, map));
                }
                else
                {
                    MappySystem.SystemConfig.FollowPlayer = false;
                    viewport.SetViewportCenter(new Vector2(1024.0f, 1024.0f));
                    viewport.SetViewportZoom(0.4f);
                }
            }

            if (MappySystem.SystemConfig.FollowOnOpen)
            {
                MappySystem.MapTextureController.MoveMapToPlayer();
                MappySystem.SystemConfig.FollowPlayer = true;
            }
        }

        ProcessingCommand = false;
        ImGui.SetWindowFocus(WindowName);
    }

    public override void Draw()
    {
        UpdateSizePosition();

        if (MappySystem.MapTextureController is not { Ready: true, MapTexture: var texture, CurrentMap: var map }) return;

        SetWindowFlags();

        Viewport.UpdateViewportStart(ImGui.GetCursorScreenPos());
        if (ImGui.BeginChild("###MapFrame", ImGui.GetContentRegionAvail(), false, Flags))
        {
            Viewport.UpdateSize();

            Viewport.SetImGuiDrawPosition();
            var scaledSize = new Vector2(texture.Width, texture.Height) * Viewport.Scale;
            ImGui.Image(texture.ImGuiHandle, scaledSize, Vector2.Zero, Vector2.One, Vector4.One with { W = GetFadePercent() });

            if (!toolbar.MapSearch.ShowMapSelectOverlay) MappySystem.ModuleController.Draw(Viewport, map);

            toolbar.Draw();
            MappySystem.ContextMenuController.Draw();
        }
        ImGui.EndChild();

        ReadMouseInputs();
    }

    public override void OnClose()
    {
        UIModule.PlaySound(24u, 0, 0, 0);

        ProcessingCommand = false;
    }

    private void UpdateSizePosition()
    {
        var systemConfig = MappySystem.SystemConfig;
        var windowPosition = ImGui.GetWindowPos();
        var windowSize = ImGui.GetWindowSize();

        if (!IsFocused)
        {
            if (windowPosition != systemConfig.WindowPosition)
            {
                ImGui.SetWindowPos(systemConfig.WindowPosition);
            }

            if (windowSize != systemConfig.WindowSize)
            {
                ImGui.SetWindowSize(systemConfig.WindowSize);
            }
        }
        else // If focused
        {
            if (systemConfig.WindowPosition != windowPosition)
            {
                systemConfig.WindowPosition = windowPosition;
                MappyPlugin.System.SaveConfig();
            }

            if (systemConfig.WindowSize != windowSize)
            {
                systemConfig.WindowSize = windowSize;
                MappyPlugin.System.SaveConfig();
            }
        }
    }

    private float GetFadePercent() => ShouldFade() ? 1.0f - MappySystem.SystemConfig.FadePercent : 1.0f;

    private bool ShouldFade()
    {
        foreach (var flag in Enum.GetValues<FadeMode>())
        {
            if (!MappySystem.SystemConfig.FadeMode.HasFlag(flag)) continue;

            switch (flag)
            {
                case FadeMode.Always:
                case FadeMode.WhenFocused when KamiCommon.WindowManager.GetWindowOfType<MapWindow>() is { IsFocused: true }:
                case FadeMode.WhenUnFocused when KamiCommon.WindowManager.GetWindowOfType<MapWindow>() is { IsFocused: false }:
                case FadeMode.WhenMoving when AgentMap.Instance()->IsPlayerMoving is 1:
                    return true;
            }
        }

        return false;
    }

    private void SetWindowFlags()
    {
        Flags = DrawFlags.DefaultFlags;

        if (MappySystem.SystemConfig.LockWindow) Flags |= DrawFlags.NoMoveResizeFlags;
        if (MappySystem.SystemConfig.HideWindowFrame) Flags |= DrawFlags.NoDecorationFlags;
        if (!Bound.IsCursorInWindowHeader()) Flags |= ImGuiWindowFlags.NoMove;
        RespectCloseHotkey = !MappySystem.SystemConfig.IgnoreEscapeKey;
    }

    private void ReadMouseInputs()
    {
        // Disable while Searching
        if (toolbar.MapSearch.ShowMapSelectOverlay) return;

        // Only allow Context, Zoom, an DragStart if cursor is over the map
        if (Bound.IsCursorInWindow() && !Bound.IsCursorInWindowHeader())
        {
            if (ImGui.IsItemHovered())
            {
                ProcessContextMenu();
                ProcessZoomChange();
            }

            // Clicking to start a drag sets hovered to false
            ProcessMapDragStart();
        }

        // Allow DragStop to be outside the map window
        ProcessMapDragStop();
    }

    private void ProcessMapDragStart()
    {
        // Don't allow a drag to start if the window size is changing
        if (ImGui.GetWindowSize() == lastWindowSize)
        {
            if (ImGui.IsItemClicked(ImGuiMouseButton.Left) && !dragStarted)
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

    private void ProcessMapDragStop()
    {
        if (ImGui.IsMouseDragging(ImGuiMouseButton.Left) && dragStarted)
        {
            var delta = mouseDragStart - ImGui.GetMousePos();
            Viewport.MoveViewportCenter(delta);
            mouseDragStart = ImGui.GetMousePos();
            ProcessZoomChange();
        }
        else if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
        {
            dragStarted = false;
        }

        if (dragStarted) MappySystem.SystemConfig.FollowPlayer = false;
    }

    private void ProcessContextMenu()
    {
        if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
        {
            MappySystem.ContextMenuController.Show(PopupMenuType.AddMoveFlag,
                PopupMenuType.ViewParentMap,
                PopupMenuType.ViewRegionMap,
                PopupMenuType.ViewSource,
                PopupMenuType.ViewFirst);
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

    // ReSharper disable once UnusedMember.Local
    [DoubleTierCommandHandler("GoToCommandHelp", "map", "goto")]
    private void GoToCommand(params string[] args)
    {
        if (args.Length is not 2) return;

        var x = float.Parse(args[0]);
        var y = float.Parse(args[1]);

        if (MappySystem.MapTextureController is { Ready: true, CurrentMap: var map })
        {
            ProcessingCommand = true;
            var worldX = Utility.Position.MapToWorld(x, map.SizeFactor, map.OffsetX);
            var worldY = Utility.Position.MapToWorld(y, map.SizeFactor, map.OffsetY);

            IsOpen = true;
            MappySystem.SystemConfig.FollowPlayer = false;

            Viewport.SetViewportCenter(new Vector2(worldX, worldY));
            Viewport.SetViewportZoom(2.0f);

            TemporaryMarkers.SetGatheringMarker(new TemporaryMapMarker
            {
                Position = new Vector2(worldX, worldY) / (map.SizeFactor / 100.0f) - new Vector2(1024.0f, 1024.0f) / (map.SizeFactor / 100.0f),
                TooltipText = "Goto Command",
                IconID = 60561, // Flag Marker
                Radius = 50.0f,
                Type = MarkerType.Command,
                MapID = map.RowId
            });
        }
    }
}