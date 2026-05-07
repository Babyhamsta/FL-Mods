# Skip Startup

Skip Startup is a small Flashing Lights ModKit v0.1.0 mod that fast-forwards the game's language loader, opens `MainMenu2`, then refreshes the menu once language content is ready.

## Design

- Uses `ModKitMelonMod<SkipStartupConfig>`.
- Keeps config minimal: `Enabled` and `VerboseLogging`.
- Uses `StartupScenePolicy` for testable scene-skip decisions.
- Uses no Harmony patches.
- Forces the Start scene `Canvas/FSM` into `Create Languages` instead of waiting for the normal logo path to reach it.
- Pins the Start scene `Languages` object with `DontDestroyOnLoad` before switching scenes.
- Reloads `MainMenu2` once after the language-content FSM reports `DONE_b` so menu labels rebuild with loaded text.

## Runtime Behavior

The exported Unity build settings show:

- `Start` at build index `0`
- `MainMenu2` at build index `1`

While scene `0` / `Start` is active, the mod fast-forwards the Start scene FSM to create the persistent language loader, records that startup was bypassed once, then calls:

```csharp
SceneManager.LoadScene(1, LoadSceneMode.Single);
```

The `Languages` object continues loading across the scene switch. Once its content FSM reports `DONE_b`, the mod reloads `MainMenu2` one time so the game's own menu text setup runs with loaded language data.
