using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SubnauticaCapsule.Patches;

[HarmonyPatch(typeof(LootDistributionData))]
internal static class LootDistributionPatch
{
    private static readonly BiomeType[] TargetBiomes =
    {
        BiomeType.SafeShallows_Grass,
        BiomeType.SafeShallows_CaveFloor,
        BiomeType.Kelp_GrassSparse,
        BiomeType.Kelp_Sand,
        BiomeType.GrassyPlateaus_Grass,
        BiomeType.GrassyPlateaus_Sand,
        BiomeType.MushroomForest_Sand,
        BiomeType.MushroomForest_Grass,
        BiomeType.SparseReef_Coral,
        BiomeType.GrandReef_Ground,
        BiomeType.DeepGrandReef_Ground,
        BiomeType.BloodKelp_Floor,
        BiomeType.Mountains_Sand,
        BiomeType.Dunes_SandDune,
        BiomeType.LostRiverJunction_Ground,
        BiomeType.LostRiverCorridor_Ground,
    };

    [HarmonyPatch(nameof(LootDistributionData.Initialize))]
    [HarmonyPostfix]
    static void Postfix(LootDistributionData __instance)
    {
        int extraCount = Plugin.Cfg.ExtraSpawnCount.Value;
        if (extraCount <= 0) return;

        string classId = CraftData.GetClassIdForTechType(TechType.TimeCapsule);
        if (string.IsNullOrEmpty(classId))
        {
            Plugin.Log.LogWarning("Could not resolve TimeCapsule class ID. Extra spawns disabled.");
            return;
        }

        float probability = Mathf.Clamp(Plugin.Cfg.SpawnProbability.Value, 0.01f, 1.0f);

        // FIX: Register TimeCapsule in srcDistribution (required by CSVEntitySpawner.GetPrefabForSlot)
        if (!__instance.srcDistribution.ContainsKey(classId))
        {
            var biomeDistribution = new List<LootDistributionData.BiomeData>();
            foreach (var biome in TargetBiomes)
            {
                biomeDistribution.Add(new LootDistributionData.BiomeData
                {
                    biome = biome,
                    count = 1,
                    probability = probability
                });
            }
            __instance.srcDistribution[classId] = new LootDistributionData.SrcData
            {
                prefabPath = "WorldEntities/Alterra/TimeCapsule",
                distribution = biomeDistribution
            };
            Plugin.Log.LogInfo($"Registered TimeCapsule in srcDistribution with {biomeDistribution.Count} biomes.");
        }

        // Diagnostic: verify WorldEntityInfo exists and log slotType
        if (UWE.WorldEntityDatabase.TryGetInfo(classId, out var entityInfo))
        {
            Plugin.Log.LogInfo($"TimeCapsule WorldEntityInfo: slotType={entityInfo.slotType}, " +
                $"techType={entityInfo.techType}, cellLevel={entityInfo.cellLevel}");
        }
        else
        {
            Plugin.Log.LogWarning("TimeCapsule classId not found in WorldEntityDatabase! " +
                "Extra capsules will fail to spawn.");
        }

        // Inject into dstDistribution
        int injected = 0;
        foreach (var biome in TargetBiomes)
        {
            if (!__instance.dstDistribution.TryGetValue(biome, out var dstData))
            {
                dstData = new LootDistributionData.DstData
                {
                    prefabs = new List<LootDistributionData.PrefabData>()
                };
                __instance.dstDistribution[biome] = dstData;
            }

            var prefabData = new LootDistributionData.PrefabData
            {
                classId = classId,
                count = 1,
                probability = probability
            };
            dstData.prefabs.Add(prefabData);
            injected++;
        }

        Plugin.Log.LogInfo($"Injected TimeCapsule (classId={classId}) into {injected} biomes " +
            $"(probability: {probability:F3} each)");
    }
}
