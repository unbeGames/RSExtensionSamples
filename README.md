This is a samples repository for [Rocket Science](https://store.steampowered.com/app/890520) extensions. It was built using [RSExtensionTemplate](https://github.com/unbeGames/RSExtensionTemplate). **Note**: extension support was added in the v0.24.x version of the game (it is currently not released to public).

## Stucture

Each subfolder contains a separate extension sample. It also contains its own README that describes concepts and API it is using.

- [RocketPartModuleExtension](RocketPartModuleExtension): contains an example how to create custom rocket part module and system and add them to existing parts. 

## General notes

- Use Unity.Mathematics for math instead of System.Math. 
- If you are using native containers, like a `NativeArray<T>` you are responsible for disposing it, otherwise you'll get a memory leak.
