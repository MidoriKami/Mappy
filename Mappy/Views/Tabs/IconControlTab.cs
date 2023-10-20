using KamiLib.Interfaces;
using Mappy.Views.General;

namespace Mappy.Views.Tabs;

public class IconControlTab : ITabItem {
    public string TabName => "Icon Control";
    public bool Enabled => true;
    private readonly IconDisableView iconDisableView = new();
    public void Draw() => iconDisableView.Draw();
}