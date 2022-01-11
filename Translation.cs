// -----------------------------------------------------------------------
// <copyright file="Translation.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel;
using Exiled.API.Interfaces;

namespace Mistaken.CustomCuffing
{
    /// <inheritdoc/>
    public class Translation : ITranslation
    {
        /// <summary>
        /// Gets or sets a message that appears when you hover on a Player when hes Cuffed.
        /// </summary>
        [Description("Message that appears when you hover on a Player when hes Cuffed")]
        public string CuffedBy { get; set; } = "<color=red>Cuffed by (cuffer)</color>";
    }
}
