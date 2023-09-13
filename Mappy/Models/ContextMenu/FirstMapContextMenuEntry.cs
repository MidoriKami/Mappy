using System.Numerics;
using Mappy.Abstracts;
using Mappy.Models.Enums;
using Mappy.System;

namespace Mappy.Models.ContextMenu;

public class FirstMapContextMenuEntry : IContextMenuEntry
{
    public PopupMenuType Type => PopupMenuType.ViewFirst;
    public string Label => "View The First";
    public bool Enabled => (MappySystem.MapTextureController.CurrentMap?.Id.RawString.StartsWith("region") ?? false) ||
                           (MappySystem.MapTextureController.CurrentMap?.Id.RawString.StartsWith("world") ?? false);
    public void ClickAction(Vector2 clickPosition)
    {
        if (MappySystem.MapTextureController is { } controller)
        {
            controller.LoadMap(535);
        }
    }
}