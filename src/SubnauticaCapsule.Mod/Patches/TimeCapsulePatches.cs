using HarmonyLib;

namespace SubnauticaCapsule.Patches;

[HarmonyPatch]
internal static class TimeCapsulePatches
{
    private static int spawnedCount;

    /// <summary>
    /// Enforce MaxCapsules limit by preventing registration beyond the cap.
    /// Capsules that exceed the limit are silently destroyed.
    /// </summary>
    [HarmonyPatch(typeof(PlayerTimeCapsule), nameof(PlayerTimeCapsule.RegisterSpawn))]
    [HarmonyPrefix]
    static bool RegisterSpawnPrefix(TimeCapsule timeCapsule)
    {
        int max = Plugin.Cfg.MaxCapsules.Value;
        if (max > 0 && spawnedCount >= max)
        {
            Plugin.Log.LogDebug($"Capsule limit reached ({max}). Skipping spawn registration.");
            timeCapsule.DoNotSpawn();
            return false;
        }
        return true;
    }

    /// <summary>
    /// Track successful spawns so we can enforce the limit.
    /// Each capsule that gets content from the API counts toward the total.
    /// </summary>
    [HarmonyPatch(typeof(TimeCapsule), nameof(TimeCapsule.Spawn))]
    [HarmonyPostfix]
    static void TrackSpawn()
    {
        spawnedCount++;
        Plugin.Log.LogDebug($"TimeCapsule spawned. Total: {spawnedCount}");
    }

    /// <summary>
    /// Reset spawn tracking when the game is unloaded (returning to menu or quitting).
    /// </summary>
    [HarmonyPatch(typeof(TimeCapsuleContentProvider), nameof(TimeCapsuleContentProvider.Deinitialize))]
    [HarmonyPostfix]
    static void ResetOnDeinitialize()
    {
        spawnedCount = 0;
        Plugin.Log.LogDebug("Capsule spawn counter reset.");
    }
}
