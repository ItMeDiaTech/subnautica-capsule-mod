using System;
using System.Collections.Generic;
using HarmonyLib;

namespace SubnauticaCapsule.Patches;

[HarmonyPatch]
internal static class TimeCapsulePatches
{
    private static int spawnedCount;
    private static readonly List<CachedContent> contentCache = new();

    private struct CachedContent
    {
        public string id;
        public TimeCapsuleContent content;
    }

    /// <summary>
    /// Enforce MaxCapsules limit by preventing registration beyond the cap.
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
    /// Cache content whenever the game stores capsule data, so we can reuse it
    /// as fallback for capsules that fail the API call.
    /// </summary>
    [HarmonyPatch(typeof(TimeCapsuleContentProvider), nameof(TimeCapsuleContentProvider.Set))]
    [HarmonyPostfix]
    static void CacheContent(string id, TimeCapsuleContent content)
    {
        if (content == null || string.IsNullOrEmpty(id)) return;

        contentCache.Add(new CachedContent { id = id, content = content });
        Plugin.Log.LogDebug($"Cached capsule content (id={id}). Cache size: {contentCache.Count}");
    }

    /// <summary>
    /// Track successful spawns.
    /// </summary>
    [HarmonyPatch(typeof(TimeCapsule), nameof(TimeCapsule.Spawn))]
    [HarmonyPostfix]
    static void TrackSpawn()
    {
        spawnedCount++;
        Plugin.Log.LogDebug($"TimeCapsule spawned. Total: {spawnedCount}");
    }

    /// <summary>
    /// Instead of destroying capsules that fail the API call, try assigning cached content.
    /// If no cached content is available, let the original DoNotSpawn proceed.
    /// </summary>
    [HarmonyPatch(typeof(TimeCapsule), nameof(TimeCapsule.DoNotSpawn))]
    [HarmonyPrefix]
    static bool DoNotSpawnFallback(TimeCapsule __instance)
    {
        if (contentCache.Count == 0)
        {
            Plugin.Log.LogDebug("No cached content available. Capsule will be destroyed.");
            return true;
        }

        int max = Plugin.Cfg.MaxCapsules.Value;
        if (max > 0 && spawnedCount >= max)
        {
            Plugin.Log.LogDebug("Capsule limit reached. Allowing destroy.");
            return true;
        }

        try
        {
            // Pick a random cached capsule to assign
            int index = UnityEngine.Random.Range(0, contentCache.Count);
            var cached = contentCache[index];

            string instanceId = Guid.NewGuid().ToString();
            TimeCapsuleContentProvider.Set(cached.id, cached.content);
            __instance.Spawn(instanceId, cached.id);

            Plugin.Log.LogInfo($"Assigned cached content to capsule (id={cached.id})");
            return false; // Skip the original DoNotSpawn (destroy)
        }
        catch (Exception ex)
        {
            Plugin.Log.LogWarning($"Failed to assign cached content: {ex.Message}");
            return true;
        }
    }

    /// <summary>
    /// Reset state when the game is unloaded.
    /// </summary>
    [HarmonyPatch(typeof(TimeCapsuleContentProvider), nameof(TimeCapsuleContentProvider.Deinitialize))]
    [HarmonyPostfix]
    static void ResetOnDeinitialize()
    {
        spawnedCount = 0;
        contentCache.Clear();
        Plugin.Log.LogDebug("Capsule tracking state reset.");
    }
}
