using FlashingLights.ModKit.Core;
using FlashingLights.SkipStartup.Config;
using FlashingLights.SkipStartup.Runtime;
using System.Reflection;

var tests = new (string Name, Action Body)[]
{
    ("Skip Startup uses managed ModKit base", SkipStartupUsesManagedBase),
    ("Skip Startup manifest matches public identity", SkipStartupManifestMatchesPublicIdentity),
    ("Skip Startup config stays minimal", SkipStartupConfigStaysMinimal),
    ("Startup policy waits for language loader", StartupPolicyWaitsForLanguageLoader),
    ("Startup policy skips Start scene when loader exists", StartupPolicySkipsStartSceneWhenLoaderExists),
    ("Startup policy fast-forwards language creation once", StartupPolicyFastForwardsLanguageCreationOnce),
    ("Startup policy refreshes MainMenu2 after early skip", StartupPolicyRefreshesMainMenu2AfterEarlySkip),
    ("Startup policy does not bypass repeatedly", StartupPolicyDoesNotBypassRepeatedly),
    ("Bootstrap probe targets language content FSM", BootstrapProbeTargetsLanguageContentFsm),
    ("Skip Startup has no legacy handoff prefix", SkipStartupHasNoLegacyHandoffPrefix)
};

var failed = 0;

foreach (var test in tests)
{
    try
    {
        test.Body();
        Console.WriteLine($"PASS {test.Name}");
    }
    catch (Exception ex)
    {
        failed++;
        Console.Error.WriteLine($"FAIL {test.Name}: {ex.GetType().Name}: {ex.Message}");
    }
}

return failed == 0 ? 0 : 1;

static void SkipStartupUsesManagedBase()
{
    AssertEqual(
        typeof(ModKitMelonMod<SkipStartupConfig>),
        typeof(SkipStartupMod).BaseType,
        "Skip Startup should register with the ModKit UI through the v0.1.0 SDK base.");
}

static void SkipStartupManifestMatchesPublicIdentity()
{
    var manifest = typeof(SkipStartupMod).GetCustomAttribute<ModKitManifestAttribute>();

    AssertNotNull(manifest, "SkipStartupMod should declare ModKit manifest metadata.");
    AssertEqual("babyhamsta.skip-startup", manifest!.Id, "Manifest id should match config path identity.");
    AssertEqual("Skip Startup", manifest.DisplayName, "Manifest display name should match public name.");
    AssertEqual("0.1.0", manifest.Version, "Manifest version should start at 0.1.0.");
    AssertEqual("Babyhamsta", manifest.Author, "Manifest author should match public repo owner.");
    AssertEqual("MIT", manifest.License, "Manifest license should match repo license.");
    AssertEqual("0.1.0", manifest.MinSdkVersion, "Manifest should target SDK v0.1.0.");
}

static void SkipStartupConfigStaysMinimal()
{
    var publicProperties = typeof(SkipStartupConfig)
        .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
        .Select(property => property.Name)
        .OrderBy(name => name, StringComparer.Ordinal)
        .ToArray();

    AssertSequenceEqual(
        new[] { "Enabled", "VerboseLogging" },
        publicProperties,
        "Skip Startup config should expose only the Core enabled toggle and verbose logging.");

    var config = new SkipStartupConfig();
    AssertEqual(true, config.Enabled, "Skip Startup should be enabled by default.");
    AssertEqual(false, config.VerboseLogging, "Verbose logging should be off by default.");
}

static void StartupPolicyWaitsForLanguageLoader()
{
    var policy = new StartupScenePolicy();

    AssertEqual(false, policy.ShouldBypassStartupScene(StartupScenePolicy.StartSceneBuildIndex, StartupScenePolicy.StartSceneName, languageLoaderAlive: false), "Start scene should not bypass before the language loader exists.");
}

static void StartupPolicySkipsStartSceneWhenLoaderExists()
{
    var policy = new StartupScenePolicy();

    AssertEqual(true, policy.ShouldBypassStartupScene(StartupScenePolicy.StartSceneBuildIndex, StartupScenePolicy.StartSceneName, languageLoaderAlive: true), "Start scene should bypass when the persistent language loader exists.");
    AssertEqual(true, policy.ShouldBypassStartupScene(99, StartupScenePolicy.StartSceneName, languageLoaderAlive: true), "Start scene name should be accepted if a runtime reports an unexpected build index.");
    AssertEqual(false, policy.HasBypassedStartupLoad, "Policy should not mark bypassed before recording.");
}

static void StartupPolicyFastForwardsLanguageCreationOnce()
{
    var policy = new StartupScenePolicy();

    AssertEqual(false, policy.HasFastForwardedLanguageCreation, "Policy should start without a language-creation fast-forward.");
    policy.RecordFastForwardedLanguageCreation();
    AssertEqual(true, policy.HasFastForwardedLanguageCreation, "Policy should remember the language-creation fast-forward.");
}

static void StartupPolicyRefreshesMainMenu2AfterEarlySkip()
{
    var policy = new StartupScenePolicy();

    AssertEqual(false, policy.ShouldReloadMainMenuAfterLanguageContent(StartupScenePolicy.MainMenuSceneBuildIndex, StartupScenePolicy.MainMenuSceneName), "MainMenu2 should not reload before the Start scene was skipped.");
    policy.RecordBypassedStartupLoad();
    AssertEqual(true, policy.ShouldReloadMainMenuAfterLanguageContent(StartupScenePolicy.MainMenuSceneBuildIndex, StartupScenePolicy.MainMenuSceneName), "MainMenu2 should reload once after the early Start scene skip.");
    AssertEqual(false, policy.ShouldReloadMainMenuAfterLanguageContent(2, "Loading"), "Unrelated scenes should not trigger a language refresh reload.");
    policy.RecordMainMenuLanguageRefresh();
    AssertEqual(true, policy.HasRefreshedMainMenuText, "Policy should remember that the menu language refresh already ran.");
    AssertEqual(false, policy.ShouldReloadMainMenuAfterLanguageContent(StartupScenePolicy.MainMenuSceneBuildIndex, StartupScenePolicy.MainMenuSceneName), "MainMenu2 should not reload repeatedly.");
}

static void StartupPolicyDoesNotBypassRepeatedly()
{
    var policy = new StartupScenePolicy();

    AssertEqual(true, policy.ShouldBypassStartupScene(StartupScenePolicy.StartSceneBuildIndex, StartupScenePolicy.StartSceneName, languageLoaderAlive: true), "First ready Start scene should request bypass.");
    policy.RecordBypassedStartupLoad();
    AssertEqual(true, policy.HasBypassedStartupLoad, "Policy should remember bypass once recorded.");
    AssertEqual(false, policy.ShouldBypassStartupScene(StartupScenePolicy.StartSceneBuildIndex, StartupScenePolicy.StartSceneName, languageLoaderAlive: true), "Policy should not request a second bypass in the same run.");
}

static void BootstrapProbeTargetsLanguageContentFsm()
{
    AssertEqual("Canvas", StartupBootstrapProbe.StartCanvasName, "Probe should target the Start scene Canvas object.");
    AssertEqual("FSM", StartupBootstrapProbe.StartCanvasFsmName, "Probe should target the Start scene bootstrap FSM.");
    AssertEqual("Create Languages", StartupBootstrapProbe.CreateLanguagesStateName, "Probe should fast-forward to the language creation state.");
    AssertEqual("Languages(Clone)", StartupBootstrapProbe.LanguagesCloneName, "Probe should check the instantiated language prefab first.");
    AssertEqual("Languages", StartupBootstrapProbe.LanguagesSceneName, "Probe should fall back to the scene object name.");
    AssertEqual("Content", StartupBootstrapProbe.ContentChildName, "Probe should target the language content FSM child.");
    AssertEqual("DONE_b", StartupBootstrapProbe.ContentDoneVariableName, "Probe should wait on the Start scene content-loaded bool.");
    AssertEqual("Language-Global_s", StartupBootstrapProbe.LanguageGlobalVariableName, "Probe should seed the Start scene language variable before forcing creation.");
    AssertEqual("Lang", StartupBootstrapProbe.LanguagePlayerPrefsKey, "Probe should use the game's language PlayerPrefs key.");
    AssertEqual("en", StartupBootstrapProbe.DefaultLanguageCode, "Probe should fall back to English if no language was saved.");

    var method = typeof(StartupBootstrapProbe).GetMethod(nameof(StartupBootstrapProbe.IsLanguageContentReady), BindingFlags.Instance | BindingFlags.Public);
    AssertNotNull(method, "StartupBootstrapProbe should expose a runtime readiness check.");
    AssertEqual(typeof(bool), method!.ReturnType, "Startup bootstrap probe should return a boolean readiness result.");

    var fastForwardMethod = typeof(StartupBootstrapProbe).GetMethod(nameof(StartupBootstrapProbe.TryFastForwardLanguageCreation), BindingFlags.Instance | BindingFlags.Public);
    AssertNotNull(fastForwardMethod, "StartupBootstrapProbe should expose a language-creation fast-forward helper.");
    AssertEqual(typeof(bool), fastForwardMethod!.ReturnType, "Startup bootstrap probe should return whether language creation was fast-forwarded.");

    var pinMethod = typeof(StartupBootstrapProbe).GetMethod(nameof(StartupBootstrapProbe.TryKeepLanguageLoaderAlive), BindingFlags.Instance | BindingFlags.Public);
    AssertNotNull(pinMethod, "StartupBootstrapProbe should expose a language-loader persistence helper.");
    AssertEqual(typeof(bool), pinMethod!.ReturnType, "Startup bootstrap probe should return whether the language loader was found and pinned.");
}

static void SkipStartupHasNoLegacyHandoffPrefix()
{
    var method = typeof(SkipStartupMod).GetMethod(
        "LoadStartProcessPrefix",
        BindingFlags.Static | BindingFlags.Public,
        binder: null,
        types: new[] { typeof(int) },
        modifiers: null);

    AssertNull(method, "Skip Startup should not expose the old LoadStartProcess prefix.");
}

static void AssertEqual<T>(T expected, T actual, string message)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"{message} Expected '{expected}', got '{actual}'.");
    }
}

static void AssertSequenceEqual<T>(IReadOnlyList<T> expected, IReadOnlyList<T> actual, string message)
{
    if (expected.Count != actual.Count)
    {
        throw new InvalidOperationException($"{message} Expected count {expected.Count}, got {actual.Count}.");
    }

    for (var index = 0; index < expected.Count; index++)
    {
        if (!EqualityComparer<T>.Default.Equals(expected[index], actual[index]))
        {
            throw new InvalidOperationException($"{message} Difference at {index}: expected '{expected[index]}', got '{actual[index]}'.");
        }
    }
}

static void AssertNotNull(object? value, string message)
{
    if (value == null)
    {
        throw new InvalidOperationException(message);
    }
}

static void AssertNull(object? value, string message)
{
    if (value != null)
    {
        throw new InvalidOperationException(message);
    }
}
