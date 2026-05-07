# Babyhamsta FL Mods

Public Flashing Lights mods built against [Flashing Lights ModKit](https://github.com/Babyhamsta/FL-ModKit/). Each mod is a normal external SDK consumer so players and modders can inspect, build, modify, and send pull requests without depending on the SDK source tree.

The root README is only the repo index and shared workflow. Per-mod behavior, config, and troubleshooting live beside each mod.

## Mods

| Mod | Summary | Min ModKit | Docs |
| --- | --- | --- | --- |
| Skip Startup | Skips startup logos and opens `MainMenu2` faster. | `0.1.0` | [README](./src/FlashingLights.SkipStartup/README.md) |

## Requirements

- Flashing Lights with MelonLoader installed.
- .NET SDK 9.x building `net6.0` projects.
- Flashing Lights ModKit extracted somewhere outside this repo. Use the version required by the mod you are building.

Command placeholders:

- `<GameRoot>`: folder that contains the game's `MelonLoader` folder.
- `<ModKitRoot>`: extracted ModKit SDK folder, such as `FlashingLightsModKit-v0.1.0`.

You can also set `FL_GAME_ROOT` and `FL_MODKIT_ROOT`, or extract the SDK under:

```text
sdk\FlashingLightsModKit-v0.1.0\
```

## Build

From this repo root:

```bash
dotnet build ./FL-Mods.sln -c Release \
  -p:GameRoot="<GameRoot>/" \
  -p:ModKitRoot="<ModKitRoot>/"
```

Run tests:

```bash
dotnet run --project ./tests/<ModProject>.Tests/<ModProject>.Tests.csproj -c Release \
  -p:GameRoot="<GameRoot>/" \
  -p:ModKitRoot="<ModKitRoot>/"
```

## Install

Copy the built mod DLL and ModKit Core DLL into the game `Mods` folder:

```text
<GameRoot>\Mods\
```

Files:

```text
src\<ModProject>\bin\Release\net6.0\<ModAssembly>.dll
<ModKitRoot>\lib\FlashingLights.ModKit.Core.dll
```

Launch the game. Press `Insert` to open the ModKit overlay and confirm the mod appears in the Mods tab.
