// -----------------------------------------------------------------------
// <copyright file="CustomCuffingHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MEC;
using Mistaken.API;
using Mistaken.API.Diagnostics;
using Mistaken.API.Extensions;
using Mistaken.API.GUI;
using Mistaken.Events;
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
            this.CuffedLimit.Clear();
            Exiled.Events.Handlers.Player.Handcuffing += this.Player_Handcuffing;
            Events.Handlers.CustomEvents.Uncuffing += this.Player_Uncuffing;
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Handcuffing -= this.Player_Handcuffing;
            Events.Handlers.CustomEvents.Uncuffing -= this.Player_Uncuffing;
        }

        private void Player_Handcuffing(HandcuffingEventArgs ev)
        {
            if (!this.CuffedLimit.ContainsKey(ev.Cuffer))
            {
                this.CuffedLimit.Add(ev.Cuffer, 0);
            }

            int limit = this.CuffedLimit[ev.Cuffer];
            if (limit >= PluginHandler.Instance.Config.CuffLimit)
            {
                ev.IsAllowed = false;
                return;
            }
            else if (this.CuffedLimit[ev.Cuffer] == 0)
            {
                Timing.RunCoroutine(this.CuffedGUI(ev.Cuffer));
            }

            this.CuffedLimit[ev.Cuffer]++;
            Timing.RunCoroutine(this.CuffedPlayerInfo(ev.Target));
        }

        private void Player_Uncuffing(Events.EventArgs.UncuffingEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;

            if (ev.UnCuffer.IsScp)
            {
                ev.IsAllowed = PluginHandler.Instance.Config.AllowScps;
            }

            if (ev.Target.Cuffer.IsNTF && ev.UnCuffer.IsNTF && ev.UnCuffer != ev.Target.Cuffer)
            {
                ev.IsAllowed = PluginHandler.Instance.Config.AllowOtherMtfs;
            }

            if (ev.IsAllowed)
                this.CuffedLimit[ev.Target.Cuffer]--;
        }

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

                    int limit = this.CuffedLimit[cuffer];
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
                    break;
                }

                yield return Timing.WaitForSeconds(1);
            }
        }
    }
}
