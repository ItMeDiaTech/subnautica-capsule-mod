using BepInEx.Configuration;

namespace SubnauticaCapsule;

internal class ModConfig
{
    public ConfigEntry<int> MaxCapsules { get; }
    public ConfigEntry<int> ExtraSpawnCount { get; }
    public ConfigEntry<int> MaxQueueSize { get; }
    public ConfigEntry<float> SpawnProbability { get; }
    public ConfigEntry<bool> DebugGlow { get; }
    public bool IsUnlimited => MaxCapsules.Value <= 0;

    public ModConfig(ConfigFile config)
    {
        MaxCapsules = config.Bind("General", "MaxCapsules", 0,
            "Maximum total time capsules per save. 0 = unlimited (default). " +
            "The queue size throttle prevents API bursts regardless of this setting.");

        ExtraSpawnCount = config.Bind("Spawning", "ExtraSpawnCount", 40,
            "Controls how many biomes receive TimeCapsule injection. " +
            "Higher values increase the probability weight per biome. " +
            "Actual capsule count depends on random biome slot resolution.");

        MaxQueueSize = config.Bind("Spawning", "MaxQueueSize", 15,
            "Maximum capsules waiting for API content at once. " +
            "Prevents bursts of requests to the server. " +
            "Capsules that exceed this are silently removed.");

        SpawnProbability = config.Bind("Spawning", "SpawnProbability", 5.0f,
            "Probability weight for TimeCapsule in each biome's loot table. " +
            "Higher values = more capsules. Range 0.01 to 5.0. Default 5.0.");

        DebugGlow = config.Bind("Debug", "DebugGlow", false,
            "Adds a bright point light to spawned time capsules for easy visibility during testing.");
    }
}
