# Mappy
[![Download count](https://img.shields.io/endpoint?url=https://qzysathwfhebdai6xgauhz4q7m0mzmrf.lambda-url.us-east-1.on.aws/Mappy)](https://github.com/MidoriKami/Mappy)

Mappy is a XivLauncher/Dalamud plugin.

Mappy is a total replacement plugin for the ingame main map.
Mappy offers a lot of customization options such as enabling and disabling specific icons, configuring colors of tooltips, or displayed areas.

Integrates seamlessly into the games built in functions, for example pressing `m` will open Mappy instead of the built in game map.

> [!IMPORTANT]  
> Mappy is not intended to replace the Minimap, there are many functions of the Minimap that Mappy does not replicate.

## Map Window

The main feature of this plugin is the map window itself. It is rendered via ImGui as an overlay ontop of the game UI.
One major benefit of this is when you have Dalamud's Multi-Monitor Mode enabled, you can move Mappy onto another display.

Among many customization options you can configure Mappy to suit your needs and preferences.

![image](https://github.com/user-attachments/assets/23cc5cd9-f20b-4298-8aea-1c1942b4f29b)

### Simple Context Menu

Upon right clicking anywhere on the map, you will see a context menu that shows some additional actions you can perform.

No more will you have to remember "wait was it alt + click to place a flag???"

![image](https://github.com/user-attachments/assets/fba25c3c-0858-4f77-8ff1-0c169d401558)


## Search Window

Pressing the search button in the main ui will open a map search window that allows you to search for any 
Map, Aetheryte, Point of Interest, or Aethernet to easily navigate to any map you wish.

Points of Interest will focus the map on the exact location of the point of interest.

Unfortunately the other search types don't include sufficient information to be able to center the map on them.

![image](https://github.com/user-attachments/assets/c34824be-0e11-4bff-a423-f18dd247433f)

## Configuration Window

Mappys configuration window allows you to customize the displayed map features however you like.

> [!TIP]
> I have done my best to set reasonable defaults for settings, but it is up to you to tailor your experience to be how you would like.

### Icon Settings

The main way that you can tailor your map to suit your needs is via the Icon Settings, here you can configure what icons show, how big they are, what color they are, and more.

You can configure any icon that the map has ever shown you, as you use Mappy more icons will become available as Mappy discovers them.

![image](https://github.com/user-attachments/assets/3f9bff5f-5ca3-4728-a1d7-6c19dcea262e)

## Quest List Window

Accessible from the context menu, the Quest List Window will show you all of the quests that you have accepted, and all the quests that are available to be claimed in the current area.

Clicking on any of these entries will focus the map on that specific quest.

| Accepted  | Unaccepted   |
|---|---|
| ![image](https://github.com/user-attachments/assets/4f40aa42-f7ab-4cb6-b1b2-7b838227142a)  | ![image](https://github.com/user-attachments/assets/127d6052-533a-47b2-8c3d-ba32033c4317)  |

## Fate List Window

Accessible from the context menu, the Fate List Window will show you all of the Fates that are currently active in the current area.

Fates that are expiring soon will have their named colored, and slowly fade from green to red the closer they get to expiring.

Fates with exp bonuses will have "Exp Bonus!" displayed.

Clicking on any entry in the Fate List Window will center the map on that fate.

![image](https://github.com/user-attachments/assets/eb41dc11-87e5-4198-afb6-f394c18b75ab)

