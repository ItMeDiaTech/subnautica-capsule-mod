using System.Collections.Generic;
using System.Threading;
using HarmonyLib;

namespace SubnauticaCapsule.Patches;

[HarmonyPatch]
internal static class TimeCapsulePatches
{
    private static int spawnedCount;

    /// <summary>
    /// Enforce MaxCapsules limit and queue size throttle.
    ///
    /// The game's SpawnRoutine processes capsules sequentially — one API POST per
    /// capsule, waiting for the response before moving to the next. This naturally
    /// limits the rate to ~1 request per 1-15 seconds. However, if many capsules
    /// register at once (e.g. several entity slots in one batch resolve to TimeCapsule),
    /// the queue could grow large, causing a sustained burst of API calls.
    ///
    /// We guard against this with two checks:
    ///   1. MaxCapsules — hard cap on total spawned capsules
    ///   2. MaxQueueSize — limit how many capsules can wait for API content at once
    /// </summary>
    [HarmonyPatch(typeof(PlayerTimeCapsule), nameof(PlayerTimeCapsule.RegisterSpawn))]
    [HarmonyPrefix]
    static bool RegisterSpawnPrefix(PlayerTimeCapsule __instance, TimeCapsule timeCapsule)
    {
        int max = Plugin.Cfg.MaxCapsules.Value;
        if (max > 0 && Interlocked.CompareExchange(ref spawnedCount, 0, 0) >= max)
        {
            Plugin.Log.LogDebug($"Capsule limit reached ({spawnedCount}/{max}). Destroying capsule.");
            timeCapsule.DoNotSpawn();
            return false;
        }

        List<TimeCapsule> queue = __instance.spawnQueue;
        if (queue != null)
        {
            int maxQueue = Plugin.Cfg.MaxQueueSize.Value;
            if (maxQueue > 0 && queue.Count >= maxQueue)
            {
                Plugin.Log.LogDebug($"Spawn queue full ({queue.Count}/{maxQueue}). Destroying capsule to avoid API burst.");
                timeCapsule.DoNotSpawn();
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Track successful spawns (capsules that received content from the API).
    /// </summary>
    [HarmonyPatch(typeof(TimeCapsule), nameof(TimeCapsule.Spawn))]
    [HarmonyPostfix]
    static void TrackSpawn()
    {
        int total = Interlocked.Increment(ref spawnedCount);
        Plugin.Log.LogDebug($"TimeCapsule spawned with content. Total: {total}");
    }

    /// <summary>
    /// Reset spawn tracking when the game is unloaded.
    /// </summary>
    [HarmonyPatch(typeof(TimeCapsuleContentProvider), nameof(TimeCapsuleContentProvider.Deinitialize))]
    [HarmonyPostfix]
    static void ResetOnDeinitialize()
    {
        Interlocked.Exchange(ref spawnedCount, 0);
        Plugin.Log.LogDebug("Capsule spawn counter reset.");
    }
}
