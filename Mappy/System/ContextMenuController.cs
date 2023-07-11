using System.Linq;
using System.Numerics;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.ContextMenu;
using Mappy.Utility;

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

    private readonly IContextMenuEntry[] generalContextMenuEntries =
    {
        new FlagContextMenuEntry(),
        new TemporaryMarkerContextMenuEntry(),
    };

    public void Show(ContextMenuType type, Viewport viewport, Map map)
    {
        menuType = type;
        
        clickPosition = Position.GetTexturePosition(ImGui.GetMousePos() - viewport.StartPosition, map, viewport);
    }

    public void Draw()
    {
        if (ImGui.BeginPopupContextWindow("###GeneralRightClickContext"))
        {
            foreach (var contextMenuEntry in generalContextMenuEntries)
            {
                if (!contextMenuEntry.Visible) continue;
                if (!contextMenuEntry.MenuTypes.Contains(menuType)) continue;

                if (ImGui.Selectable(contextMenuEntry.Label))
                {
                    contextMenuEntry.ClickAction(clickPosition);
                }
            }

            ImGui.EndPopup();
        }
    }
}
