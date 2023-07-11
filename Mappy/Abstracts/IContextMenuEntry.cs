using System.Numerics;
using Mappy.System;

namespace Mappy.Abstracts;

public interface IContextMenuEntry
{
    ContextMenuType[] MenuTypes { get; }
    
    bool Visible { get; }
    
    string Label { get; }

    void ClickAction(Vector2 clickPosition);
}