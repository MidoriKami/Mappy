using DailyDuty;
using KamiLib.AutomaticUserInterface;
using KamiLib.Interfaces;
using Mappy.System;

namespace Mappy.Views.Tabs;

public class GeneralConfigurationTab : ITabItem
{
    public string TabName => "General";
    public bool Enabled => true;
    public void Draw() => DrawableAttribute.DrawAttributes(MappySystem.SystemConfig, MappyPlugin.System.SaveConfig);
}