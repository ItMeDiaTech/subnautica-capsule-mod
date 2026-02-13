using HarmonyLib;
using UnityEngine;

namespace SubnauticaCapsule.Patches;

[HarmonyPatch(typeof(TimeCapsule))]
internal static class TimeCapsuleGlowPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    static void AddGlow(TimeCapsule __instance)
    {
        if (!Plugin.Cfg.DebugGlow.Value) return;

        var light = __instance.gameObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(0.2f, 1f, 0.5f);
        light.intensity = 3f;
        light.range = 30f;
        light.shadows = LightShadows.None;

        Plugin.Log.LogDebug($"Added debug glow to TimeCapsule at {__instance.transform.position}");
    }
}
