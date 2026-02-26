![Example](assets/logo.png)
# CustomButtonHints

- Adds custom buttons to the in-game ButtonWindow UI

| Compatibility           | Platform  | Tested game version |
| ----------------------- | --------- | ------------------- |
| Megastore Simulator     | Steam     | v0.2.1             |

## Installation

Just follow these 3 easy steps:

1. If you haven't already, install Tobey's BepInEx Pack for Megastore Simulator - make sure to read the Quick Start for easy instructions:
    - [Tobey's BepInEx Megastore Simulator Git](https://github.com/toebeann/BepInEx.MegastoreSimulator#quick-start) or [Nexus page](https://www.nexusmods.com/megastoresimulator/mods/2)
2. Download the latest release of from the [releases page](https://github.com/FluffyNinjaKitty/CustomButtonHints/releases).
3. Extract the `BepInEx` folder from the downloaded zip file into your game folder - Just drag the `BepInEx` folder out into your game folder

## How to use for your mods

### First off, make sure you set this mod's file as a dependency
 - Add my `CustomButtonHints.dll` as a project dependency to your project
 - You will need to add either of these lines at the start of your mod's main Plugin.cs script
```cs
[BepInDependency("CustomButtonHints", BepInDependency.DependencyFlags.SoftDependency)]

[BepInDependency("CustomButtonHints", BepInDependency.DependencyFlags.HardDependency)]
```
Depending on if your mod absolutely needs it or not, or if you'd like to add your own warning if it isn't installed

Right below your plugin info, like this
```cs
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("CustomButtonHints", BepInDependency.DependencyFlags.SoftDependency)]
```


### Second, the functions that will be available to you:
```cs
public static void AddCustomAction(string actionName, string actionText, KeyCode keyCode) //Adds a custom action for use
public static void AddCustomAction(string actionName, string actionText, ConfigEntry<KeyCode> entry) //Adds a custom action for use, using a ConfigEntry so the key can be refreshed in-game using a ConfigurationManager
public static void AddButtonToUI(string actionName, List<string> existingButtons, Action functionCall, bool exactMatch = true) //Adds a custom action to the UI matching the given list of existing buttons
public static void AddButtonToUI(string actionName, Action functionCall) //Forcefully adds a custom button to the UI on the next redraw
public static void RemoveButtonFromUI(string actionName) //Makes sure a custom button is not added on the next redraw of the UI
public static void RemoveButtonFromUI(KeyCode keyCode) //Makes sure a vanilla button is not added on the next redraw of the UI
public static void OpenButtonWindow() //Forcefully opens and draws the ButtonWindow, useful if you want it to be manually opened
public static void CloseButtonWindow() //Forcefully closes the ButtonWindow, careful with this one. It can cause softlocks if used incorrectly
```

### Adding your actions to the game
You need to start by using the `AddCustomAction` function first. Without that you won't be able to add them to the UI

Examples:
```cs
AddCustomAction("myaction_name", "Button display text", KeyCode.B);
```
or
```cs
AddCustomAction("myaction_name", "Button display text", myConfig.myConfigEntry); //Make sure to reference the ConfigEntry<KeyCode> itself, not the value
```
This will add an action using the KeyCode `B` with the name `myaction_name` and will display `Button display text`

With the former being a direct KeyCode, and the latter being a ConfigEntry. Using a config entry will allow it to be changed mid-game using a configuration manager
>[!NOTE]
>Every ConfigEntry<KeyCode> added to the list will get an event handler added to it to refresh the lists for UI usage
>In addition a keybind with a default of F10 can be used to manually refresh

When added to the UI, will look like this:

![Example](assets/CustomButtonExample.png)

### How to add them to the UI, and make them do something
> [!NOTE]
> It is completely YOUR responsibility to make sure you are adding and removing buttons properly

Using the same example above, you can add that action to the UI using either of the `AddButtonToUI()` functions
```cs
    ButtonHints.AddButtonToUI("myaction_name", ["pack", "set_price", "place_move"], delegate { ExampleFunction(); }, true);
```
This line tells the mod to add the action `myaction_name` to the UI if the actions `pack` `set_price` `place_move` are in the list.

These are added by a function in the game when looking at a shelf with empty hands, using a tool like DnSpy, you can find these.

This, for example, is the call adding them in `Shelf.OnMouseHoverStarted()`
```cs
SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, ValueTuple<string, Action>>
{
    {
        KeyCode.F,
        new ValueTuple<string, Action>("pack", delegate
        {
            this.parentPlaceable.Pack();
        })
    },
    {
        KeyCode.T,
        new ValueTuple<string, Action>("set_price", new Action(this.OnSetPrice))
    },
    {
        KeyCode.Mouse1,
        new ValueTuple<string, Action>("place_move", delegate
        {
            this.parentPlaceable.StartNewPlacement(null);
        })
    }
}, base.transform, true);
```
This can be used to find updates to the UI based on what the game is already adding to it.

A different way is using the other method for `AddButtonToUI()`
```cs
    AddButtonToUI("myaction_name", delegate { ExampleFunction(); });
```
This adds the button to a list of actions that will be added to the UI regardless of exsisting content next time it draws.

Next is the `RemoveButtonFromUI()` function. It behaves similar to the second method of adding, where it adds it to a list to be removed. Making sure it does NOT draw on the next update to the UI

First one will remove any custom actions added
```cs
    RemoveButtonFromUI("myaction_name");
```
Second one will remove any vanilla actions added
```cs
    RemoveButtonFromUI(KeyCode.C);
```

The last two functions are straight forward, `OpenButtonWindow()` calls the function to open the UI using an empty list, and if you have added any using the `AddButtonToUI()` functions, they will be added.

And `CloseButtonWindow()` closes the UI and clears all actions from the lists to add/remove actions

## Need help?

You can use the following links to ask for help:
 - [Modded Megastore Simulator Discord](https://discord.gg/9KrRZx7akG)
 - [Nexus Mods page](https://www.nexusmods.com/megastoresimulator/mods/)
