using System.Collections.Generic;
using System.Linq;
using KamiLib.Utilities;
using Mappy.Abstracts;

namespace Mappy.System;

public class ModuleController
{
    public readonly List<ModuleBase> Modules;
    
    public ModuleController()
    {
        Modules = Reflection.ActivateOfType<ModuleBase>().ToList();
    }

    public void Load()
    {
        foreach (var module in Modules)
        {
            module.Load();
        }
    }

    public void Unload()
    {
        foreach (var module in Modules)
        {
            module.Unload();
        }
    }
}