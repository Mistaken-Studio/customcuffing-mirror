// -----------------------------------------------------------------------
// <copyright file="CustomCuffingHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MEC;
using Mistaken.API;
using Mistaken.API.Diagnostics;
using Mistaken.API.Extensions;
using Mistaken.API.GUI;
using System.Collections.Generic;
using UnityEngine;

namespace Mistaken.CustomCuffing
{
    internal class CustomCuffingHandler : Module
    {
        public CustomCuffingHandler(PluginHandler plugin)
    : base(plugin)
        {
        }

        public override string Name => "CustomCuffingHandler";

        private readonly Dictionary<Player, int> CuffedLimit = new Dictionary<Player, int>();

        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.Handcuffing += this.Player_Handcuffing;
            //Exiled.Events.Handlers.Player.RemovingHandcuffs += this.Player_RemovingHandcuffs;
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Handcuffing -= this.Player_Handcuffing;
            //Exiled.Events.Handlers.Player.RemovingHandcuffs -= this.Player_RemovingHandcuffs;
        }

        private void Player_Handcuffing(HandcuffingEventArgs ev)
        {
            if (!CuffedLimit.ContainsKey(ev.Cuffer))
            {
                CuffedLimit.Add(ev.Cuffer, 0);
            }
            int limit = CuffedLimit[ev.Cuffer];
            if (limit >= PluginHandler.Instance.Config.CuffLimit)
            {
                ev.IsAllowed = false;
                return;
            }
            else if (CuffedLimit[ev.Cuffer] == 0)
            {
                Timing.RunCoroutine(CuffedGUI(ev.Cuffer));
            }
            CuffedLimit[ev.Cuffer]++;
            Timing.RunCoroutine(CuffedPlayerInfo(ev.Target));
        }

        /*private void Player_RemovingHandcuffs(RemovingHandcuffsEventArgs ev)
        {
            if (ev.Cuffer.IsScp)
            {
                ev.IsAllowed = PluginHandler.Instance.Config.AllowScps;
            }
            if (ev.Target.Cuffer.IsNTF && ev.Cuffer.IsNTF && ev.Cuffer != ev.Target.Cuffer)
            {
                ev.IsAllowed = PluginHandler.Instance.Config.AllowOtherMtfs;
            }
        }*/

        private IEnumerator<float> CuffedGUI(Player cuffer)
        {
            yield return Timing.WaitForSeconds(1);
            while (cuffer.IsConnected && cuffer.IsAlive)
            {
                try
                {
                    List<string> cuffed = new List<string>();
                    foreach (Player target in Player.List)
                    {
                        if (target.IsAlive && target.IsCuffed && target.Cuffer == cuffer)
                        {
                            var distance = Vector3.Distance(cuffer.Position, target.Position);
                            cuffed.Add($"<color=yellow>{target.Nickname}</color> - <color=yellow>{Mathf.RoundToInt(distance)}</color>m away");
                        }
                    }

                    CuffedLimit.TryGetValue(cuffer, out int limit);
                    if (cuffed.Count != 0)
                    {
                        cuffer.SetGUI($"cuffer-{cuffer.Nickname}", PseudoGUIPosition.BOTTOM, $"Cuffed Players: (<color=yellow>{limit}/{PluginHandler.Instance.Config.CuffLimit}</color>)<br><br>{string.Join("<br>", cuffed)}");
                    }
                    else
                    {
                        cuffer.SetGUI($"cuffer-{cuffer.Nickname}", PseudoGUIPosition.BOTTOM, null);
                        break;
                    }
                }
                catch (System.Exception ex)
                {
                    this.Log.Error(ex);
                }
                yield return Timing.WaitForSeconds(1);
            }

            cuffer.SetGUI($"cuffer-{cuffer.Nickname}", PseudoGUIPosition.BOTTOM, null);
        }

        private IEnumerator<float> CuffedPlayerInfo(Player target)
        {
            yield return Timing.WaitForSeconds(1);
            while (target.IsConnected && target.IsAlive)
            {
                if (target.IsCuffed)
                {
                    CustomInfoHandler.Set(target, $"cuffed-{target.Nickname}", PluginHandler.Instance.Translation.CuffedBy.Replace("{cuffer}", target.Cuffer.Nickname));
                }
                else
                {
                    CustomInfoHandler.Set(target, $"cuffed-{target.Nickname}", string.Empty);
                    CuffedLimit[target.Cuffer]--;
                    break;
                }
                yield return Timing.WaitForSeconds(1);
            }
        }
    }
}
