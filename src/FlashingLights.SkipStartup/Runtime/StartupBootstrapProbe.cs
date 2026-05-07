using Il2Cpp;
using UnityEngine;

namespace FlashingLights.SkipStartup.Runtime;

public sealed class StartupBootstrapProbe
{
    public const string StartCanvasName = "Canvas";
    public const string StartCanvasFsmName = "FSM";
    public const string CreateLanguagesStateName = "Create Languages";
    public const string LanguagesCloneName = "Languages(Clone)";
    public const string LanguagesSceneName = "Languages";
    public const string ContentChildName = "Content";
    public const string ContentDoneVariableName = "DONE_b";
    public const string LanguageGlobalVariableName = "Language-Global_s";
    public const string LanguagePlayerPrefsKey = "Lang";
    public const string DefaultLanguageCode = "en";

    public bool TryFastForwardLanguageCreation()
    {
        if (IsLanguageLoaderAlive())
        {
            return false;
        }

        var startCanvas = GameObject.Find(StartCanvasName);
        if (startCanvas == null)
        {
            return false;
        }

        var startFsm = PlayMakerFSM.FindFsmOnGameObject(startCanvas, StartCanvasFsmName) ?? startCanvas.GetComponent<PlayMakerFSM>();
        if (startFsm == null)
        {
            return false;
        }

        var createLanguagesState = startFsm.Fsm.GetState(CreateLanguagesStateName);
        if (createLanguagesState == null)
        {
            return false;
        }

        var languageCode = PlayerPrefs.GetString(LanguagePlayerPrefsKey, DefaultLanguageCode);
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            languageCode = DefaultLanguageCode;
        }

        // Let the game's own PlayMaker state create the language prefab instead of duplicating asset-loading logic.
        startFsm.FsmVariables.GetFsmString(LanguageGlobalVariableName).Value = languageCode;
        startFsm.Fsm.SwitchState(createLanguagesState);
        return true;
    }

    public bool TryKeepLanguageLoaderAlive()
    {
        var languagesRoot = FindLanguageRoot();
        if (languagesRoot == null)
        {
            return false;
        }

        UnityEngine.Object.DontDestroyOnLoad(languagesRoot);
        return true;
    }

    public bool IsLanguageLoaderAlive()
    {
        return FindLanguageRoot() != null;
    }

    public bool IsLanguageContentReady()
    {
        var contentTransform = FindLanguageContent();
        if (contentTransform == null)
        {
            return false;
        }

        var contentFsm = contentTransform.GetComponent<PlayMakerFSM>();
        if (contentFsm == null)
        {
            return false;
        }

        // Start scene localization is PlayMaker-driven; this flag is the build-setting-safe signal that menu text exists.
        var contentLoaded = contentFsm.FsmVariables.GetFsmBool(ContentDoneVariableName);
        return contentLoaded != null && contentLoaded.Value;
    }

    private static GameObject? FindLanguageRoot()
    {
        return GameObject.Find(LanguagesCloneName) ?? GameObject.Find(LanguagesSceneName);
    }

    private static Transform? FindLanguageContent()
    {
        var languagesRoot = FindLanguageRoot();
        return languagesRoot == null ? null : languagesRoot.transform.Find(ContentChildName);
    }
}
