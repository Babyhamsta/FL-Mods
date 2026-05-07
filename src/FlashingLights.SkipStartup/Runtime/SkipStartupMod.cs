using FlashingLights.ModKit.Core;
using FlashingLights.SkipStartup.Config;
using UnityEngine.SceneManagement;

namespace FlashingLights.SkipStartup.Runtime;

[ModKitManifest(
    Id = "babyhamsta.skip-startup",
    DisplayName = "Skip Startup",
    Version = "0.1.0",
    Author = "Babyhamsta",
    License = "MIT",
    Changelog = "Skip startup scene flow and open the main menu immediately.",
    MinSdkVersion = "0.1.0",
    Category = "Quality of Life",
    Tags = new[] { "startup", "menu", "quality-of-life" })]
public sealed class SkipStartupMod : ModKitMelonMod<SkipStartupConfig>
{
    private readonly StartupBootstrapProbe startupBootstrapProbe = new();
    private readonly StartupScenePolicy startupScenePolicy = new();
    private bool loggedBootstrapProbeFailure;
    private bool loggedWaitingForLanguageContent;
    private bool loggedWaitingForLanguageLoader;

    protected override string ModId => "babyhamsta.skip-startup";

    protected override void OnModKitUpdate()
    {
        var activeScene = SceneManager.GetActiveScene();
        if (startupScenePolicy.ShouldCheckStartupScene(activeScene.buildIndex, activeScene.name))
        {
            JumpToMainMenuWhenLanguageLoaderExists(activeScene.buildIndex, activeScene.name);
            return;
        }

        if (startupScenePolicy.ShouldReloadMainMenuAfterLanguageContent(activeScene.buildIndex, activeScene.name))
        {
            ReloadMainMenuWhenLanguageContentIsReady();
        }
    }

    private void JumpToMainMenuWhenLanguageLoaderExists(int buildIndex, string sceneName)
    {
        FastForwardLanguageCreation();
        var languageLoaderAlive = TryKeepLanguageLoaderAlive();
        if (!startupScenePolicy.ShouldBypassStartupScene(buildIndex, sceneName, languageLoaderAlive))
        {
            if (!languageLoaderAlive && !loggedWaitingForLanguageLoader)
            {
                loggedWaitingForLanguageLoader = true;
                LogDebug("Waiting for Start scene language loader before opening MainMenu2.");
            }

            return;
        }

        startupScenePolicy.RecordBypassedStartupLoad();
        LogInfo("Start scene language loader is alive; loading MainMenu2 while language content finishes.");
        SceneManager.LoadScene(StartupScenePolicy.MainMenuSceneBuildIndex, LoadSceneMode.Single);
    }

    private void FastForwardLanguageCreation()
    {
        if (startupScenePolicy.HasFastForwardedLanguageCreation)
        {
            return;
        }

        try
        {
            if (startupBootstrapProbe.TryFastForwardLanguageCreation())
            {
                startupScenePolicy.RecordFastForwardedLanguageCreation();
                LogInfo("Fast-forwarded Start scene language creation.");
            }
        }
        catch (Exception ex)
        {
            LogBootstrapProbeFailure(ex);
        }
    }

    private void ReloadMainMenuWhenLanguageContentIsReady()
    {
        if (!IsStartupBootstrapReady())
        {
            if (!loggedWaitingForLanguageContent)
            {
                loggedWaitingForLanguageContent = true;
                LogDebug("Waiting for language content before refreshing MainMenu2 text.");
            }

            return;
        }

        startupScenePolicy.RecordMainMenuLanguageRefresh();
        LogInfo("Language content is ready; reloading MainMenu2 to refresh localized text.");
        SceneManager.LoadScene(StartupScenePolicy.MainMenuSceneBuildIndex, LoadSceneMode.Single);
    }

    private bool TryKeepLanguageLoaderAlive()
    {
        try
        {
            return startupBootstrapProbe.TryKeepLanguageLoaderAlive();
        }
        catch (Exception ex)
        {
            LogBootstrapProbeFailure(ex);
        }

        return false;
    }

    private bool IsStartupBootstrapReady()
    {
        try
        {
            return startupBootstrapProbe.IsLanguageContentReady();
        }
        catch (Exception ex)
        {
            LogBootstrapProbeFailure(ex);
        }

        return false;
    }

    private void LogBootstrapProbeFailure(Exception ex)
    {
        if (loggedBootstrapProbeFailure)
        {
            return;
        }

        loggedBootstrapProbeFailure = true;
        LogWarning($"Could not inspect Start scene language bootstrap; leaving normal startup flow active. {ex.GetType().Name}: {ex.Message}");
    }
}
