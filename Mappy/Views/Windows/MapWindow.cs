using System.Numerics;
using Dalamud.Interface.Windowing;
using KamiLib;
using Mappy.System;

namespace Mappy.Views.Windows;

public class MapWindow : Window
{
    public MapWindow() : base("Mappy - Map Window")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(470,200),
            MaximumSize = new Vector2(9999,9999)
        };

        IsOpen = true;
        
        KamiCommon.WindowManager.AddWindow(this);
    }

    public override bool DrawConditions()
    {
        if (!Service.ClientState.IsLoggedIn) return false;
        if (Service.ClientState.IsPvP) return false;

        return true;
    }

    public override void Draw() => MappySystem.ModuleController.Draw();
}