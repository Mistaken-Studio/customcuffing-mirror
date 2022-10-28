// -----------------------------------------------------------------------
// <copyright file="PluginHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using HarmonyLib;
using Mistaken.Updater.API.Config;

namespace Mistaken.CustomCuffing
{
    internal sealed class PluginHandler : Plugin<Config, Translation>, IAutoUpdateablePlugin
    {
        public override string Author => "Mistaken Devs";

        public override string Name => "CustomCuffing";

        public override string Prefix => "MCustomCuffing";

        public override PluginPriority Priority => PluginPriority.Medium;

        public override Version RequiredExiledVersion => new(5, 2, 2);

        public AutoUpdateConfig AutoUpdateConfig => new()
        {
            Type = SourceType.GITLAB,
            Url = "https://git.mistaken.pl/api/v4/projects/80",
        };

        public override void OnEnabled()
        {
            Instance = this;

            _harmony.PatchAll();

            new CustomCuffingHandler(this);

            Events.Handlers.CustomEvents.LoadedPlugins += this.CustomEvents_LoadedPlugins;

            API.Diagnostics.Module.OnEnable(this);

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            _harmony.UnpatchAll();

            Events.Handlers.CustomEvents.LoadedPlugins -= this.CustomEvents_LoadedPlugins;

            API.Diagnostics.Module.OnDisable(this);

            base.OnDisabled();
        }

        internal static PluginHandler Instance { get; private set; }

        private static readonly Harmony _harmony = new("com.mistaken.customcuffing");

        private void CustomEvents_LoadedPlugins()
        {
            if (Exiled.Loader.Loader.Plugins.Any(x => x.Name == "BetterSCP-SCP049"))
                BetterScp049Integration.Init();
        }
    }
}
