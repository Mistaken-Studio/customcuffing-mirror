using HarmonyLib;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;

namespace Mistaken.CustomCuffing;

internal sealed class Plugin
{
    public static Plugin Instance { get; private set; }

    public static readonly Translations Translations = new();

    [PluginConfig]
    public Config Config;

    [PluginPriority(LoadPriority.Medium)]
    [PluginEntryPoint("Custom Cuffing", "1.0.0", "Plugin that ", "Mistaken Devs")]
    public void Load()
    {
        Instance = this;
        _harmony.PatchAll();
        new CustomCuffingHandler();
    }

    [PluginUnload]
    public void Unload()
    {
    }

    private static readonly Harmony _harmony = new("com.mistaken.customcuffing");

    /*private void CustomEvents_LoadedPlugins()
    {
        if (Exiled.Loader.Loader.Plugins.Any(x => x.Name == "BetterSCP-SCP049"))
            BetterScp049Integration.Init();
    }*/
}
