This is a template project for [Rocket Science](https://store.steampowered.com/app/890520) extensions. **Note**: extension support was added in the v0.24.x version of the game (it is currently not released to public).

## Getting started

You need to follow a few simple steps to get started:

- clone the repository or downlad a zip archive;
- set GamePath variable to the path to the Rocket Science folder in the RSExtension.csproj file;
- open RSExtensionTemplate.sln using Visual studio;
- you are ready to go!

Don't forget to rename project and `MyExtension.cs` file to something more meaningfull that will represent your extension functionality.

## Structure

You need to inherit you first class from `BaseExtention` class from `UnbeGames.API` namespace for your extension to be recognized by the game. This template already contains such class in the `MyExtension.cs`, but you can create your one and have more than one in the project. This will be an entry point to your extension. The next step is to override some virtual methods provided in the `BaseExtention` class depending on what you want. Here are those methods:

- `OnStart` is called after all game systems and other extentions were loaded and initialized;
- `OnEnable` is called when mod was enabled by player or right after initialization if it was already enabled;
- `OnDisable` is called when mod was disabled by the player from the mods window;
- `OnGameLoad` is called after game save was loaded;
- `OnGameLoad` is called right after game was unloaded.

There are three more methods to override. Those are game loop callbacks:

- `Simulate(double time, double dt)` called every simulation step when game is loaded, can be exected several times in one frame.
- `Update` standard Unity callback, called every frame;
- `LateUpdate` standard Unity callback, called every frame right before rendering starts.

**Note**: that if any of those overriden methods throw an exception at any point, your mod will be automatically deregistered and will not continue to receive method calls from the game. You need to handle all exceptions or better, just prevent them from happening at all, because an exceotion handling is very slow.

