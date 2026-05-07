namespace FlashingLights.SkipStartup.Runtime;

public sealed class StartupScenePolicy
{
    public const int StartSceneBuildIndex = 0;
    public const int MainMenuSceneBuildIndex = 1;
    public const string StartSceneName = "Start";
    public const string MainMenuSceneName = "MainMenu2";

    public bool HasBypassedStartupLoad { get; private set; }

    public bool HasFastForwardedLanguageCreation { get; private set; }

    public bool HasRefreshedMainMenuText { get; private set; }

    public bool ShouldCheckStartupScene(int buildIndex, string sceneName)
    {
        if (HasBypassedStartupLoad)
        {
            return false;
        }

        return buildIndex == StartSceneBuildIndex ||
            string.Equals(sceneName, StartSceneName, StringComparison.Ordinal);
    }

    public bool ShouldBypassStartupScene(int buildIndex, string sceneName, bool languageLoaderAlive)
    {
        return languageLoaderAlive && ShouldCheckStartupScene(buildIndex, sceneName);
    }

    public bool ShouldReloadMainMenuAfterLanguageContent(int buildIndex, string sceneName)
    {
        if (!HasBypassedStartupLoad || HasRefreshedMainMenuText)
        {
            return false;
        }

        return buildIndex == MainMenuSceneBuildIndex ||
            string.Equals(sceneName, MainMenuSceneName, StringComparison.Ordinal);
    }

    public void RecordBypassedStartupLoad()
    {
        HasBypassedStartupLoad = true;
    }

    public void RecordFastForwardedLanguageCreation()
    {
        HasFastForwardedLanguageCreation = true;
    }

    public void RecordMainMenuLanguageRefresh()
    {
        HasRefreshedMainMenuText = true;
    }
}
