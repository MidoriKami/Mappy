using System.Numerics;
using Mappy.Abstracts;
using Mappy.Models.Enums;
using Mappy.System.Localization;
using Mappy.System.Modules;

namespace Mappy.Models.ContextMenu;

public class MarkedAreaContextMenuEntry : IContextMenuEntry
{
    public bool Enabled => true;
    public PopupMenuType Type => PopupMenuType.TempArea;

    public string Label => Strings.RemoveGatheringArea;
    
    public void ClickAction(Vector2 clickPosition)
    {
        TemporaryMarkers.RemoveGatheringMarker();
    }
}