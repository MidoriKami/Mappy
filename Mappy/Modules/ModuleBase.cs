using Mappy.Classes;

namespace Mappy.Modules;

public abstract class ModuleBase {
    public abstract bool ProcessMarker(MarkerInfo markerInfo);
}