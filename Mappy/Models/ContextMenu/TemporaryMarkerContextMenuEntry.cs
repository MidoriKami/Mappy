using System.Numerics;
using Mappy.Abstracts;
using Mappy.Models.Enums;
using Mappy.System;
using Mappy.System.Localization;
using Mappy.System.Modules;

namespace Mappy.Models.ContextMenu;

public class TemporaryMarkerContextMenuEntry : IContextMenuEntry
{
    public ContextMenuType[] MenuTypes => new[]
    {
        ContextMenuType.Flag,
        ContextMenuType.GatheringArea,
        ContextMenuType.Quest,
        ContextMenuType.Command,
    };
    
    public bool Visible => true;
    public string Label => TemporaryMarkers.TempMapMarker?.Type switch
    {
        MarkerType.Command => Strings.RemoveCommandMarker,
        MarkerType.Flag => Strings.RemoveFlag,
        MarkerType.Gathering => Strings.RemoveGatheringArea,
        MarkerType.Quest => Strings.RemoveQuestMarker,
        _ => "Unknown Marker Type"
    };
    
    public void ClickAction(Vector2 clickPosition)
    {
        TemporaryMarkers.RemoveMarker();
    }
}