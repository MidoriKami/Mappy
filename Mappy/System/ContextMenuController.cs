using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using Mappy.Abstracts;
using Mappy.Models.ContextMenu;
using Mappy.Models.Enums;

namespace Mappy.System;

public class ContextMenuController
{
    private readonly List<IContextMenuEntry> entries = new()
    {
        new FlagContextMenuEntry(),
        new MarkedAreaContextMenuEntry(),
        new AddMoveFlagContextMenuEntry(),
        new ParentMapContextMenuEntry(),
        new ViewRegionMapContextMenuEntry(),
        new SourceMapContextMenuEntry(),
        new FirstMapContextMenuEntry(),
    };

    private readonly HashSet<PopupMenuType> activeTypes = new();

    private bool wasWindowOpened;
    
    public void Show(params PopupMenuType[] types)
    {
        foreach (var type in types)
        {
            activeTypes.Add(type);
        }
    }

    public void Draw()
    {
        if (ImGui.BeginPopupContextWindow())
        {
            if (activeTypes.Count is 0)
            {
                ImGui.CloseCurrentPopup();
            }
            else
            {
                wasWindowOpened = true;

                if (activeTypes.Contains(PopupMenuType.TempFlag)) activeTypes.Remove(PopupMenuType.AddMoveFlag);
            
                foreach (var entry in entries.Where(contextEntry => activeTypes.Contains(contextEntry.Type)))
                {
                    entry.Draw();
                }
            }

            ImGui.EndPopup();
        }
        
        if (wasWindowOpened && !ImGui.IsPopupOpen(string.Empty, ImGuiPopupFlags.AnyPopup) && !ImGui.IsMouseClicked(ImGuiMouseButton.Right))
        {
            activeTypes.Clear();
            wasWindowOpened = false;
        }
    }
}
