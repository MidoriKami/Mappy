using System.Numerics;
using Dalamud.Interface.Windowing;

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
    }
    
    public override void Draw()
    {
        
    }
}