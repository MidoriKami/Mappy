using DailyDuty;
using KamiLib.AutomaticUserInterface;
using KamiLib.Interfaces;
using Mappy.System;
using Mappy.System.Localization;

namespace Mappy.Views.Tabs;

public class GeneralConfigurationTab : ITabItem
{
    public string TabName => Strings.General;
    public bool Enabled => true;
    public void Draw() => DrawableAttribute.DrawAttributes(MappySystem.SystemConfig, MappyPlugin.System.SaveConfig);
}