// -----------------------------------------------------------------------
// <copyright file="CustomCuffingHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Exiled.API.Extensions;
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

        public override void OnEnable()
        {
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
            int currentCuffed = this.GetCuffedPlayers(ev.Cuffer).Count();
            int limit = 0;

            if (ev.Cuffer.HasItem(ItemType.ArmorLight))
                limit = 1;
            else if (ev.Cuffer.HasItem(ItemType.ArmorCombat))
                limit = 2;
            else if (ev.Cuffer.HasItem(ItemType.ArmorHeavy))
                limit = 4;

            if (currentCuffed >= limit)
            {
                ev.IsAllowed = false;
                return;
            }

            this.Log.Debug($"Cuffer: {ev.Cuffer.ReferenceHub.playerMovementSync.PlayerVelocity} ({ev.Cuffer.ReferenceHub.playerMovementSync.PlayerVelocity != Vector3.zero})", PluginHandler.Instance.Config.VerbouseOutput);
            this.Log.Debug($"Target: {ev.Target.ReferenceHub.playerMovementSync.PlayerVelocity} ({ev.Target.ReferenceHub.playerMovementSync.PlayerVelocity != Vector3.zero})", PluginHandler.Instance.Config.VerbouseOutput);
            if (ev.Cuffer.ReferenceHub.playerMovementSync.PlayerVelocity != Vector3.zero || ev.Target.ReferenceHub.playerMovementSync.PlayerVelocity != Vector3.zero)
            {
                ev.IsAllowed = false;
                this.Log.Debug("MOVING TARGETS, NOT GOOD :/", PluginHandler.Instance.Config.VerbouseOutput);
                return;
            }

            if (currentCuffed == 0)
            {
                Timing.RunCoroutine(this.CuffedGUI(ev.Cuffer));
            }

            Timing.RunCoroutine(this.CuffedPlayerInfo(ev.Target));
        }

        private void Player_Uncuffing(Events.EventArgs.UncuffingEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;

            if (ev.UnCuffer.IsScp)
            {
                ev.IsAllowed = PluginHandler.Instance.Config.AllowScps;
                if (!ev.IsAllowed)
                    ev.UnCuffer.SetGUI($"uncuffer-{ev.UnCuffer.Nickname}", PseudoGUIPosition.MIDDLE, "<color=red>You can't uncuff someone as an SCP!</color>", 5);
            }

            if (ev.Target.Cuffer.IsNTF && (ev.UnCuffer?.IsNTF ?? false) && ev.UnCuffer != ev.Target.Cuffer)
            {
                ev.IsAllowed = PluginHandler.Instance.Config.AllowOtherMtfs;
                if (!ev.IsAllowed)
                    ev.UnCuffer.SetGUI($"uncuffer-{ev.UnCuffer.Nickname}", PseudoGUIPosition.MIDDLE, "<color=red>You can't uncuff someone who you did not handcuff!</color>", 5);
            }
        }

        private IEnumerator<float> CuffedGUI(Player cuffer)
        {
            yield return Timing.WaitForSeconds(1);
            while (cuffer.IsConnected && cuffer.IsAlive)
            {
                try
                {
                    List<string> cuffed = new List<string>();
                    int currentCuffed = 0;
                    foreach (Player target in this.GetCuffedPlayers(cuffer))
                    {
                        var distance = Vector3.Distance(cuffer.Position, target.Position);
                        cuffed.Add($"<color=yellow>{target.Nickname}</color> - <color=yellow>{Mathf.RoundToInt(distance)}</color>m away");
                        currentCuffed++;
                    }

                    int limit = 0;

                    if (cuffer.HasItem(ItemType.ArmorLight))
                        limit = 1;
                    else if (cuffer.HasItem(ItemType.ArmorCombat))
                        limit = 2;
                    else if (cuffer.HasItem(ItemType.ArmorHeavy))
                        limit = 4;

                    while (currentCuffed > limit)
                    {
                        foreach (Player target in Player.List)
                        {
                            if (target.IsAlive && target.IsCuffed && target.Cuffer == cuffer)
                            {
                                currentCuffed--;
                                target.RemoveHandcuffs();
                                break;
                            }
                        }
                    }

                    if (cuffed.Count != 0)
                    {
                        cuffer.SetGUI($"cuffer-{cuffer.Nickname}", PseudoGUIPosition.BOTTOM, $"Cuffed Players: (<color=yellow>{currentCuffed}/{limit}</color>)<br><br>{string.Join("<br>", cuffed)}");
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

        private IEnumerable<Player> GetCuffedPlayers(Player cuffer)
            => RealPlayers.List.Where(x => x.IsAlive && x.Cuffer == cuffer);
    }
}
