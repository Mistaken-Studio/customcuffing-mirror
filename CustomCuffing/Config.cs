// -----------------------------------------------------------------------
// <copyright file="Config.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel;
using Mistaken.Updater.Config;

namespace Mistaken.CustomCuffing
{
    /// <inheritdoc/>
    public class Config : IAutoUpdatableConfig
    {
        /// <inheritdoc/>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether debug should be displayed.
        /// </summary>
        [Description("If true then debug will be displayed")]
        public bool VerbouseOutput { get; set; }

        /// <inheritdoc/>
        [Description("Auto Update Settings")]
        public System.Collections.Generic.Dictionary<string, string> AutoUpdateConfig { get; set; }

        /// <summary>
        /// Gets or sets a value indicating how many people one player can disarm.
        /// </summary>
        [Description("How many people one player can disarm")]
        public int CuffLimit { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value indicating whether SCPs can release someone when they are cuffed.
        /// </summary>
        [Description("Can Scps release disarmed players")]
        public bool AllowScps { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether MTFs can release someone when they are cuffed by another MTF Unit.
        /// </summary>
        [Description("Can MTFs release someone when they are cuffed by another MTF Unit")]
        public bool AllowOtherMtfs { get; set; } = true;
    }
}
