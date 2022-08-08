// -----------------------------------------------------------------------
// <copyright file="BetterScp049Integration.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Exiled.API.Features;
using Mistaken.API.Diagnostics;

namespace Mistaken.CustomCuffing
{
    internal static class BetterScp049Integration
    {
        public static bool IsActive { get; set; } = false;

        public static Dictionary<Player, Player> Disarmed049 { get; set; } = new Dictionary<Player, Player>();

        public static void Init()
        {
            IsActive = true;
            Disarmed049 = Mistaken.BetterSCP.SCP049.Commands.DisarmCommand.DisarmedScps;
            Mistaken.BetterSCP.SCP049.Commands.DisarmCommand.Cuffed049 += DisarmCommand_Cuffed049;
        }

        private static void DisarmCommand_Cuffed049(object sender, (Player Cuffer, Player Scp049) e)
        {
            CustomCuffingHandler.Instance.RunCoroutine(CustomCuffingHandler.CufferGUI(e.Cuffer), "BetterScp049Integration.cuffed049");
            CustomCuffingHandler.Instance.RunCoroutine(CustomCuffingHandler.CuffedPlayerInfo(e.Scp049), "BetterScp049Integration.cuffed049");
        }
    }
}
