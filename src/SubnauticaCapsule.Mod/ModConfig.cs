using BepInEx.Configuration;

namespace SubnauticaCapsule;

internal class ModConfig
{
    public ConfigEntry<int> MaxCapsules { get; }
    public ConfigEntry<bool> EnableExtraSpawns { get; }
    public ConfigEntry<int> MaxQueueSize { get; }
    public ConfigEntry<float> SpawnProbability { get; }
    public ConfigEntry<bool> DebugGlow { get; }
    public bool IsUnlimited => MaxCapsules.Value <= 0;

    public ModConfig(ConfigFile config)
    {
        MaxCapsules = config.Bind("General", "MaxCapsules", 0,
            new ConfigDescription(
                "Maximum total time capsules per save. 0 = unlimited (default). " +
                "The queue size throttle prevents API bursts regardless of this setting.",
                new AcceptableValueRange<int>(0, 10000)));

        EnableExtraSpawns = config.Bind("Spawning", "EnableExtraSpawns", true,
            "Enable injecting TimeCapsule into biome loot tables for additional spawns.");

        MaxQueueSize = config.Bind("Spawning", "MaxQueueSize", 15,
            new ConfigDescription(
                "Maximum capsules waiting for API content at once. " +
                "Prevents bursts of requests to the server. " +
                "Capsules that exceed this are silently removed.",
                new AcceptableValueRange<int>(0, 100)));

        SpawnProbability = config.Bind("Spawning", "SpawnProbability", 5.0f,
            new ConfigDescription(
                "Probability weight for TimeCapsule in each biome's loot table. " +
                "Higher values = more capsules.",
                new AcceptableValueRange<float>(0.01f, 10.0f)));

        DebugGlow = config.Bind("Debug", "DebugGlow", false,
            "Adds a bright point light to spawned time capsules for easy visibility during testing.");
    }
}
