using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace SubnauticaCapsule;

[BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
public class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; private set; }
    internal static ModConfig Cfg { get; private set; }

    private void Awake()
    {
        Log = Logger;
        Cfg = new ModConfig(Config);
        new Harmony(PluginInfo.GUID).PatchAll();
        Logger.LogInfo($"{PluginInfo.Name} v{PluginInfo.Version} loaded. " +
            $"Capsule limit: {(Cfg.IsUnlimited ? "Unlimited" : Cfg.MaxCapsules.Value.ToString())}");
    }
}
