using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Mappy.Abstracts;
using Mappy.Models.Enums;
using Mappy.System.Localization;
using Mappy.System.Modules;

namespace Mappy.Models.ContextMenu; 

public unsafe class RemoveFlagContextMenuEntry : IContextMenuEntry {
    public PopupMenuType Type => PopupMenuType.RemoveFlag;
    public string Label => Strings.RemoveFlag;
    public bool Enabled => AgentMap.Instance()->IsFlagMarkerSet is 1;
    public void ClickAction(Vector2 clickPosition) => TemporaryMarkers.RemoveFlagMarker();
}