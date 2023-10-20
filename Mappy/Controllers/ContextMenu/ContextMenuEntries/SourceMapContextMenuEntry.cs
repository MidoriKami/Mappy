using System.Numerics;
using Mappy.Abstracts;
using Mappy.Models.Enums;
using Mappy.System;

namespace Mappy.Models.ContextMenu;

public class SourceMapContextMenuEntry : IContextMenuEntry {
    public PopupMenuType Type => PopupMenuType.ViewSource;
    public string Label => "View The Source";
    public bool Enabled => (MappySystem.MapTextureController.CurrentMap?.Id.RawString.StartsWith("region") ?? false) ||
                           (MappySystem.MapTextureController.CurrentMap?.Id.RawString.StartsWith("world") ?? false);
    public void ClickAction(Vector2 clickPosition) {
        if (MappySystem.MapTextureController is { } controller) {
            controller.LoadMap(384);
        }
    }
}