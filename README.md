# MonkeSwim
A mod for gorilla tag to move around in zero gravity on supported maps. To move around on any map that supports the mod, simply press the trigger and swing your arms as if you were swimming.

the mod works by searching the map, after its loaded, for an object with the name **"AirSwimConfig"**. if this object is found the mod will initialize using the values in **transform.localPosition** for its settings.
- **localPostion.x** = the max speed you can go
- **localPosition.y** = the accelleration applied to your arm swing
- **localposition.x** = the drag value (untested but i'm assuming this sslows you down over time)

# Adding support to your map
to add support to your map, add a blank **GameObject** to your map called **"AirSwimConfig"**, this will enable the mod accross the entire map with the settings defined in its position as seen in the editor.
for more options you can add these as child objects to your **AirSwimConfig** object:
- **"AirSwimConfigDefault"** - this will set the settings based on the current games base settings of max speed 6.5, accelleration of 1.1 and a drag value of 0.
- **"AirSwimConfigGlobal"** - this lets you define your own settings that will be used accross multiple zones, use this objects position for those settings.
- **"WaterSwimTriggers"** - this is used to know individual zones are being used, add your zones as child objects to this object, the child object position will be used for the settings of that zone unless AirSwimConfigGlobal, or, AirSwimConfigDefault, is present.

# Setting up individual zones
Setting up individual zones is rather simple, add a blank **GameObject** as a child to **WaterSwimTriggers**, this objects position is used for the settings of this zone, add another child to this new GameObject and attach a collider to it with trigger enabled. use this objects transform to place to zone where ever you want in the map.

# examples:
- default:

![default example](https://raw.githubusercontent.com/AHauntedArmy/MonkeSwim/master/images/default%20example.PNG)

- using gloabl custom settings accross every zone:

![global custom settings example](https://raw.githubusercontent.com/AHauntedArmy/MonkeSwim/master/images/globalconfig%20example.PNG)

- using individual zones with unique settings:

![unique individual zone settings](https://raw.githubusercontent.com/AHauntedArmy/MonkeSwim/master/images/multiple%20zones%20example.PNG)

# release
release can be found [here](https://github.com/AHauntedArmy/MonkeSwim/releases/tag/V0.1.0.2) with an included map i used for testing. to install create a folder in your plugins folder and copy over AirSwim.dll. for the map copy it into your custom maps folder.
