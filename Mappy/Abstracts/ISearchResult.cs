using System.Drawing;
using System.Numerics;
using ImGuiNET;
using KamiLib;
using KamiLib.Caching;
using KamiLib.Utilities;
using Mappy.System;
using Mappy.Views.Windows;

namespace Mappy.Abstracts;

public interface ISearchResult
{
    string Label { get; set; }
    string SubLabel { get; set; }
    uint IconId { get; set; }
    Vector2 MapPosition { get; set; }
    float MapZoom { get; set; }
    uint MapId { get; set; }

    bool DrawEntry()
    {
        if (IconCache.Instance.GetIcon(IconId) is not { } icon) return false;

        var cursorPosition = ImGui.GetCursorPos();
        ImGui.SetCursorPos(cursorPosition with { Y = cursorPosition.Y + 4.0f });
        if (ImGui.Selectable($"##{Label}{MapId}"))
        {
            Invoke();
            return true;
        }
        
        ImGui.SameLine();
        ImGui.SetCursorPos(cursorPosition);
        ImGui.Image(icon.ImGuiHandle, new Vector2(24.0f, 24.0f));
        
        ImGui.SameLine();
        ImGui.TextUnformatted(Label);

        ImGui.SameLine();
        ImGui.TextColored(KnownColor.Gray.AsVector4() with { W = 0.45f }, SubLabel);

        return false;
    }

    void Invoke()
    {
        if (MappySystem.MapTextureController is not { Ready: true } textureController) return;
        if (KamiCommon.WindowManager.GetWindowOfType<MapWindow>() is not { Viewport: var viewport }) return;

        MappySystem.SystemConfig.FollowPlayer = false;
        textureController.LoadMap(MapId);
        viewport.SetViewportCenter(MapPosition);
        viewport.SetViewportZoom(MapZoom);
    }
}