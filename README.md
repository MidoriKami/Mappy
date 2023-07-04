# Mappy
[![Download count](https://img.shields.io/endpoint?url=https://vz32sgcoal.execute-api.us-east-1.amazonaws.com/Mappy)](https://github.com/MidoriKami/Mappy)

Mappy is a XivLauncher/Dalamud plugin.

Mappy is a total replacement plugin for the ingame main map. This plugin is not intended to replace the games minimap.
Mappy offers a lot of customization options such as enabling and disabling specific icons, configuring colors of tooltips, or displayed areas.
Integrates seamlessly into the games built in functions, for example pressing `m` will open Mappy instead of the built in game map.

![image](https://github.com/MidoriKami/Mappy/assets/9083275/02d79ece-f2ba-458f-9ea4-c59500c19674)

![image](https://github.com/MidoriKami/Mappy/assets/9083275/a1328788-4fca-49a1-883f-420820e81682)

![image](https://github.com/MidoriKami/Mappy/assets/9083275/41357ae5-5e1d-4b27-8cc4-a409bf6042b9)

![image](https://github.com/MidoriKami/Mappy/assets/9083275/574c6494-796c-44f6-8229-dc05beec1784)

# Mappy IPC
Mappy provides various functions to other plugins to allow interplugin communication, and example usage is illustrated below.

```cs
    private readonly List<string> registeredMarkers = new();

    // Copy/Paste this to subscribe to these functions, be sure to check for IPCNotReady exceptions ;)
    public ICallGateSubscriber<uint, Vector2, uint, string, string, string>? AddWorldMarkerIpcFunction = null;
    public ICallGateSubscriber<uint, Vector2, uint, string, string, string>? AddTextureMarkerIpcFunction = null;
    public ICallGateSubscriber<uint, Vector2, uint, string, string, string>? AddMapCoordinateMarkerIpcFunction = null;
    public ICallGateSubscriber<string, bool>? RemoveMarkerIpcFunction = null;
    public ICallGateSubscriber<string, Vector2, bool>? UpdateMarkerIpcFunction = null;
    public ICallGateSubscriber<Vector2, Vector2, uint, Vector4, float, string>? AddTextureLineIpcFunction = null;
    public ICallGateSubscriber<Vector2, Vector2, uint, Vector4, float, string>? AddMapCoordLineIpcFunction = null;
    public ICallGateSubscriber<string, bool>? RemoveArrowMarkerIpcFunction = null;
    public ICallGateSubscriber<bool>? IsReadyIpcFunction = null;
```

```cs
        // Copy/Paste this to subscribe to these functions, be sure to check for IPCNotReady exceptions ;)
        AddWorldMarkerIpcFunction = Service.PluginInterface.GetIpcSubscriber<uint, Vector2, uint, string, string, string>("Mappy.World.AddMarker");
        AddTextureMarkerIpcFunction = Service.PluginInterface.GetIpcSubscriber<uint, Vector2, uint, string, string, string>("Mappy.Texture.AddMarker");
        AddMapCoordinateMarkerIpcFunction = Service.PluginInterface.GetIpcSubscriber<uint, Vector2, uint, string, string, string>("Mappy.MapCoord.AddMarker");
        RemoveMarkerIpcFunction = Service.PluginInterface.GetIpcSubscriber<string, bool>("Mappy.RemoveMarker");
        UpdateMarkerIpcFunction = Service.PluginInterface.GetIpcSubscriber<string, Vector2, bool>("Mappy.UpdateMarker");
        AddTextureLineIpcFunction = Service.PluginInterface.GetIpcSubscriber<Vector2, Vector2, uint, Vector4, float, string>("Mappy.Texture.AddLine");
        AddMapCoordLineIpcFunction = Service.PluginInterface.GetIpcSubscriber<Vector2, Vector2, uint, Vector4, float, string>("Mappy.MapCoord.AddLine");
        RemoveArrowMarkerIpcFunction = Service.PluginInterface.GetIpcSubscriber<string, bool>("Mappy.RemoveArrowMarker");
        IsReadyIpcFunction = Service.PluginInterface.GetIpcSubscriber<bool>("Mappy.IsReady");
```

Be sure to save the ID's of the generated markers so you can remove them when you are done!
```cs
        registeredMarkers.Add(AddWorldMarkerIpcFunction.InvokeFunc(6011, new Vector2(0.0f, 0.0f), 0, "TestMarker", "TestDescription"));
        registeredMarkers.Add(AddMapCoordinateMarkerIpcFunction.InvokeFunc(60011, new Vector2(25.0f, 9.9f), 700, "TestMarker", "TestDescription\nWithMultiline"));
        registeredMarkers.Add(AddMapCoordLineIpcFunction.InvokeFunc(new Vector2(25.0f, 9.9f), new Vector2(33.7f, 15.2f), 700, KnownColor.Aqua.AsVector4(), 2.0f));
        registeredMarkers.Add(AddMapCoordinateMarkerIpcFunction.InvokeFunc(60011, new Vector2(33.7f, 15.2f), 700, "TestMarker", "TestDescription\nWithMultiline"));
```

```cs
        foreach (var registered in registeredMarkers)
        {
            RemoveMarkerIpcFunction?.InvokeFunc(registered);
        }
```


![image](https://github.com/MidoriKami/Mappy/assets/9083275/328eb0c8-3243-4803-b336-faf982f4d291)
