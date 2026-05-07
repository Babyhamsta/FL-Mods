# Babyhamsta FL Mods

Public Flashing Lights mods built against Flashing Lights ModKit v0.1.0. Each mod is a normal external SDK consumer so players and modders can inspect, build, modify, and send pull requests without depending on the SDK source tree.

## Mods

| Mod | Project | Status |
| --- | --- | --- |
| Skip Startup | `src/FlashingLights.SkipStartup` | Initial public mod |

## Requirements

- Flashing Lights with MelonLoader installed.
- .NET SDK 9.x building `net6.0` projects.
- Flashing Lights ModKit v0.1.0 extracted somewhere outside this repo.

Use these placeholders in commands:

- `<GameRoot>`: folder that contains the game's `MelonLoader` folder.
- `<ModKitRoot>`: extracted `FlashingLightsModKit-v0.1.0` SDK folder.

You can also set `FL_GAME_ROOT` and `FL_MODKIT_ROOT`, or extract the SDK to:

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
dotnet run --project ./tests/FlashingLights.SkipStartup.Tests/FlashingLights.SkipStartup.Tests.csproj -c Release \
  -p:GameRoot="<GameRoot>/" \
  -p:ModKitRoot="<ModKitRoot>/"
```

## Install

Copy the built mod DLL and the ModKit Core DLL into the game `Mods` folder:

```text
<GameRoot>\Mods\
```

Files:

```text
src\FlashingLights.SkipStartup\bin\Release\net6.0\FlashingLights.SkipStartup.dll
<ModKitRoot>\lib\FlashingLights.ModKit.Core.dll
```

Launch the game. Press `Insert` to open the ModKit overlay and confirm the mod appears in the Mods tab.

## Skip Startup

Skip Startup fast-forwards the `Start` scene language loader, opens `MainMenu2`, then refreshes the menu once language content is ready so players reach the main menu faster without breaking localized menu text.

The mod uses the game's build settings discovered from the exported Unity project:

- `0` = `Start`
- `1` = `MainMenu2`

Config path:

```text
<GameRoot>\UserData\FlashingLightsModKit\babyhamsta.skip-startup.json
```

Config fields:

- `Enabled`: lets the ModKit overlay enable or disable the mod.
- `VerboseLogging`: prints skip decisions to the MelonLoader console when enabled.

Troubleshooting:

- If the DLL is in `Mods` but the mod is not listed by MelonLoader, check `Properties/AssemblyInfo.cs` for `MelonInfo` and `MelonGame` metadata.
- If the mod appears but startup still plays, open the ModKit overlay and confirm `Enabled` is true.
- If a game update changes scene build indexes or the Start scene language FSM, update `StartupScenePolicy` and `StartupBootstrapProbe` together.
