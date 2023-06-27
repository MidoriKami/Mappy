using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;
using KamiLib;
using Mappy.System.Localization;
using Mappy.System.Modules;
using Mappy.Utility;
using Mappy.Views.Windows;

namespace Mappy.System;

public enum ContextMenuType
{
    Inactive,
    General,
    Flag,
    GatheringArea,
}

public class ContextMenuController
{
    private Vector2 clickPosition;
    private ContextMenuType menuType;

    public void Show(ContextMenuType type)
    {
        if (KamiCommon.WindowManager.GetWindowOfType<MapWindow>() is not { } mapWindow) return;
        if (MappySystem.MapTextureController is not { Ready: true, CurrentMap: var map }) return;
        
        menuType = type;
        
        clickPosition = Position.GetTexturePosition(ImGui.GetMousePos() - mapWindow.MapContentsStart, map, mapWindow.Viewport);
    }

    public void Draw()
    {
        if (KamiCommon.WindowManager.GetWindowOfType<MapWindow>() is { IsFocused: false })
        {
            menuType = ContextMenuType.Inactive;
        }

        switch (menuType)
        {
            case ContextMenuType.General:
                DrawGeneralContext();
                break;
            
            case ContextMenuType.Flag:
                DrawFlagContext();
                break;
            
            case ContextMenuType.GatheringArea:
                DrawGatheringContext();
                break;
        }
    }

    private unsafe void DrawGeneralContext()
    {
        if (ImGui.BeginPopupContextWindow("###GeneralRightClickContext"))
        {
            var label = AgentMap.Instance()->IsFlagMarkerSet is 0 ? Strings.AddFlag : Strings.MoveFlag; 
            
            if (ImGui.Selectable(label))
            {
                if(MappySystem.MapTextureController is {Ready: true, CurrentMap: var map})
                {
                    var agent = AgentMap.Instance();
                    agent->IsFlagMarkerSet = 0;
                    agent->SetFlagMapMarker(map.TerritoryType.Row, map.RowId, clickPosition.X, clickPosition.Y);

                    if (MappySystem.SystemConfig.InsertFlagInChat)
                    {
                        MappySystem.GameIntegration.InsertFlagInChat();
                    }
                }
            }
            ImGui.EndPopup();
        }
    }

    private void DrawFlagContext()
    {
        if (ImGui.BeginPopupContextWindow("###FlagContext"))
        {
            if (ImGui.Selectable(Strings.RemoveFlag))
            {
                Flag.RemoveFlagMarker();
            }

            ImGui.EndPopup();
        }
    }

    private void DrawGatheringContext()
    {
        if (ImGui.BeginPopupContextWindow("###GatheringContext"))
        {
            if (ImGui.Selectable(Strings.RemoveGatheringArea))
            {
                GatheringArea.RemoveGatheringAreaMarker();
            }

            ImGui.EndPopup();
        }
    }
}
