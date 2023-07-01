using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;
using KamiLib;
using Mappy.Models.Enums;
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
    Command,
    Quest,
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
            
            case ContextMenuType.Flag or ContextMenuType.GatheringArea or ContextMenuType.Quest or ContextMenuType.Command:
                DrawTemporaryMarkerContext();
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
                        AgentChatLog.Instance()->InsertTextCommandParam(1048, false);
                    }
                }
            }
            ImGui.EndPopup();
        }
    }

    private void DrawTemporaryMarkerContext()
    {
        if (ImGui.BeginPopupContextWindow("###TemporaryMarkerContext"))
        {
            var label = TemporaryMarkers.TempMapMarker?.Type switch
            {
                MarkerType.Command => Strings.RemoveCommandMarker,
                MarkerType.Flag => Strings.RemoveFlag,
                MarkerType.Gathering => Strings.RemoveGatheringArea,
                MarkerType.Quest => Strings.RemoveQuestMarker,
                _ => "Unknown Marker Type"
            };

            if (ImGui.Selectable(label))
            {
                TemporaryMarkers.RemoveMarker();
            }

            ImGui.EndPopup();
        }
    }
}
