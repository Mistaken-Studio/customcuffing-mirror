using Exiled.API.Features;
using Mistaken.API.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mistaken.CustomCuffing
{
    internal class BetterScp049Integration
    {
        public static bool IsActive = false;

        public static Dictionary<Player, Player> Disarmed049
        {
            get => Mistaken.BetterSCP.SCP049.Commands.DisarmCommand.DisarmedScps;
        }

        private Action<(Player, Player)> cuffed049 = Mistaken.BetterSCP.SCP049.Commands.DisarmCommand.Cuffing049 = (values) =>
        {
            Module.RunSafeCoroutine(CustomCuffingHandler.CufferGUI(values.Item1), "BetterScp049Integration.cuffed049");
            Module.RunSafeCoroutine(CustomCuffingHandler.CuffedPlayerInfo(values.Item2), "BetterScp049Integration.cuffed049");
        };
    }
}
