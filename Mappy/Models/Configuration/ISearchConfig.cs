using KamiLib.AutomaticUserInterface;

namespace Mappy.Models;

[Category("Search", 5)]
public interface ISearchConfig
{
    [BoolConfig("UseRegionSearch", "UseRegionSearchHelp")]
    public bool UseRegionSearch { get; set; }
}