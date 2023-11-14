using System.Collections.Generic;
using System.Linq;
using KamiLib.Interfaces;
using Mappy.System;
using Mappy.System.Localization;
using Mappy.Views.Components;

namespace Mappy.Views.Tabs;

public class ModuleConfigurationTab : ISelectionWindowTab {
    public string TabName => Strings.Modules;
    public ISelectable? LastSelection { get; set; }
    public IEnumerable<ISelectable> GetTabSelectables() => MappySystem.ModuleController.Modules.Select(module => new ModuleSelectable(module));
}