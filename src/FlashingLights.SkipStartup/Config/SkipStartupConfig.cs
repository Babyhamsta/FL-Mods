using FlashingLights.ModKit.Core;

namespace FlashingLights.SkipStartup.Config;

public sealed class SkipStartupConfig
{
    [ModKitConfigDisplay("Enabled")]
    public bool Enabled { get; set; } = true;

    [ModKitConfigDisplay("Verbose logging")]
    public bool VerboseLogging { get; set; } = false;
}
