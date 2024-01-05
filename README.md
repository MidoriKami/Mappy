# Mappy
[![Download count](https://img.shields.io/endpoint?url=https://qzysathwfhebdai6xgauhz4q7m0mzmrf.lambda-url.us-east-1.on.aws/Mappy)](https://github.com/MidoriKami/Mappy)

Mappy is a XivLauncher/Dalamud plugin.

Mappy is a total replacement plugin for the ingame main map. This plugin is not intended to replace the games minimap.
Mappy offers a lot of customization options such as enabling and disabling specific icons, configuring colors of tooltips, or displayed areas.
Integrates seamlessly into the games built in functions, for example pressing `m` will open Mappy instead of the built in game map.

**Mappy is not intended to replace the vanilla minimap**

**Please do not attempt to use Mappy as a Minimap**

![image](https://github.com/MidoriKami/Mappy/assets/9083275/02d79ece-f2ba-458f-9ea4-c59500c19674)

![image](https://github.com/MidoriKami/Mappy/assets/9083275/a1328788-4fca-49a1-883f-420820e81682)

![image](https://github.com/MidoriKami/Mappy/assets/9083275/41357ae5-5e1d-4b27-8cc4-a409bf6042b9)

![image](https://github.com/MidoriKami/Mappy/assets/9083275/f02ffcd2-b290-4764-8a1a-9a6e9843e576)

# Mappy IPC
Mappy provides various functions to other plugins to allow interplugin communication.

The functional API is documented here: [IpcController](https://github.com/MidoriKami/Mappy/blob/master/Mappy/Controllers/IpcController.cs)

A demonstration is documented here: [IpcDemoWindow](https://github.com/MidoriKami/Mappy/blob/master/Mappy/Views/Windows/IpcDemoWindow.cs)

While ingame you can use the command `/mappy ipcdemo` to open the IPC Demo Window, and see how the IPC works with Mappy.

_**Functions that add markers return UUID's, be sure to save the ID's of the generated markers so you can remove them when you are done!**_

Failure to do so means that when your plugin unloads, it won't clear any markers that you have told Mappy to draw.

![image](https://github.com/MidoriKami/Mappy/assets/9083275/160308b3-287c-4103-812d-08bef3277658)
