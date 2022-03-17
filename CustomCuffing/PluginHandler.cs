// -----------------------------------------------------------------------
// <copyright file="PluginHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;

namespace Mistaken.CustomCuffing
{
    /// <inheritdoc/>
    public class PluginHandler : Plugin<Config, Translation>
    {
        /// <inheritdoc/>
        public override string Author => "Mistaken Devs";

        /// <inheritdoc/>
        public override string Name => "CustomCuffing";

        /// <inheritdoc/>
        public override string Prefix => "MCustomCuffing";

        /// <inheritdoc/>
        public override PluginPriority Priority => PluginPriority.Medium;

        /// <inheritdoc/>
        public override Version RequiredExiledVersion => new Version(4, 2, 2);

        /// <inheritdoc/>
        public override void OnEnabled()
        {
            Instance = this;

            new CustomCuffingHandler(this);

            API.Diagnostics.Module.OnEnable(this);
            Events.Handlers.CustomEvents.LoadedPlugins += this.CustomEvents_LoadedPlugins;

            base.OnEnabled();
        }

        /// <inheritdoc/>
        public override void OnDisabled()
        {
            API.Diagnostics.Module.OnDisable(this);
            Events.Handlers.CustomEvents.LoadedPlugins -= this.CustomEvents_LoadedPlugins;

            base.OnDisabled();
        }

        internal static PluginHandler Instance { get; private set; }

        private void CustomEvents_LoadedPlugins()
        {
            if (Exiled.Loader.Loader.Plugins.Any(x => x.Name == "BetterSCP-SCP049"))
                BetterScp049Integration.IsActive = true;
        }
    }
}
