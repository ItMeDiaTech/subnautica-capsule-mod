using BepInEx.Configuration;

namespace SubnauticaCapsule;

internal class ModConfig
{
    public ConfigEntry<int> MaxCapsules { get; }
    public ConfigEntry<int> ExtraSpawnCount { get; }
    public bool IsUnlimited => MaxCapsules.Value <= 0;

    public ModConfig(ConfigFile config)
    {
        MaxCapsules = config.Bind("General", "MaxCapsules", 0,
            "Maximum time capsules per save. 0 = unlimited.");
        ExtraSpawnCount = config.Bind("General", "ExtraSpawnCount", 40,
            "Number of additional capsule spawn points to inject into the world beyond the base 40.");
    }
}
