using KamiLib.AutomaticUserInterface;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.System;

namespace Mappy.Abstracts;

public abstract class ModuleBase
{
    public abstract ModuleName ModuleName { get; init; }
    public abstract ModuleConfigBase Configuration { get; protected set; }

    public virtual void Load()
    {
        Configuration = MappySystem.Config.ModuleConfigurations[ModuleName];
    }

    public virtual void Unload() { }

    public void DrawConfig() => DrawableAttribute.DrawAttributes(Configuration, SaveConfig);

    private void SaveConfig()
    {
        MappySystem.Config.ModuleConfigurations[ModuleName] = Configuration;
        MappySystem.Config.Save();
    }
}