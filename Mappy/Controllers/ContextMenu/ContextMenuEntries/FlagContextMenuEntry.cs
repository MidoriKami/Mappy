using System.Numerics;
using Mappy.Abstracts;
using Mappy.Models.Enums;
using Mappy.System.Localization;
using Mappy.System.Modules;

namespace Mappy.Models.ContextMenu;

public class FlagContextMenuEntry : IContextMenuEntry {
    public bool Enabled => true;
    public PopupMenuType Type => PopupMenuType.TempFlag;
    public string Label => Strings.RemoveFlag;

    public void ClickAction(Vector2 clickPosition)
        => TemporaryMarkers.RemoveFlagMarker();
}