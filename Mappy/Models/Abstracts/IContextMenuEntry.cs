using System.Numerics;
using ImGuiNET;
using KamiLib;
using Mappy.Models.Enums;
using Mappy.System;
using Mappy.Utility;
using Mappy.Views.Windows;

namespace Mappy.Abstracts;

public interface IContextMenuEntry {
    PopupMenuType Type { get; }
    string Label { get; }
    bool Enabled { get; }
    void ClickAction(Vector2 clickPosition);
    
    void Draw() {
        if (!Enabled) return;
        if (MappySystem.MapTextureController is not { Ready: true, CurrentMap: var map }) return;
        if (KamiCommon.WindowManager.GetWindowOfType<MapWindow>() is not { Viewport: var viewport }) return;

        if (ImGui.MenuItem(Label)) {
            var clickPosition = Position.GetRawTexturePosition(ImGui.GetMousePosOnOpeningCurrentPopup() - viewport.StartPosition, map, viewport);

            ClickAction(clickPosition);
        }
    }
}