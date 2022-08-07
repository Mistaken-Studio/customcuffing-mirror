// -----------------------------------------------------------------------
// <copyright file="BetterScp049Integration.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Exiled.API.Features;
using Mistaken.API.Diagnostics;

namespace Mistaken.CustomCuffing
{
    internal static class BetterScp049Integration
    {
        static BetterScp049Integration()
        {
            if (IsActive)
    {
                Disarmed049 = Mistaken.BetterSCP.SCP049.Commands.DisarmCommand.DisarmedScps;
                Cuffed049 = Mistaken.BetterSCP.SCP049.Commands.DisarmCommand.Cuffing049;
            }
            else
        {
                Disarmed049 = new Dictionary<Player, Player>();
            }
        }

        public static bool IsActive { get; set; } = false;

        public static Dictionary<Player, Player> Disarmed049 { get; set; }

        public static Action<(Player, Player)> Cuffed049 { get; set; } = (values) =>
        {
            Module.RunSafeCoroutine(CustomCuffingHandler.CufferGUI(values.Item1), "BetterScp049Integration.cuffed049");
            Module.RunSafeCoroutine(CustomCuffingHandler.CuffedPlayerInfo(values.Item2), "BetterScp049Integration.cuffed049");
        };
    }
}
