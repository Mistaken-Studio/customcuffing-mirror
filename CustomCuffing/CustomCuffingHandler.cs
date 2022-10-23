// -----------------------------------------------------------------------
// <copyright file="CustomCuffingHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MEC;
using Mistaken.API;
using Mistaken.API.Diagnostics;
using Mistaken.API.Extensions;
using Mistaken.API.GUI;
using UnityEngine;

namespace Mistaken.CustomCuffing
{
    internal sealed class CustomCuffingHandler : Module
    {
        public static IEnumerator<float> CufferGUI(Player cuffer)
        {
            yield return Timing.WaitForSeconds(1);
            while (cuffer.IsConnected && cuffer.IsAlive)
            {
                try
                {
                    List<string> cuffed = new();
                    int currentCuffed = 0;

                    foreach (Player target in GetCuffedPlayers(cuffer))
                    {
                        // var distance = Vector3.Distance(cuffer.Position, target.Position);
                        // cuffed.Add(string.Format(PluginHandler.Instance.Translation.CufferElementOfListInfo, target.GetDisplayName(), Mathf.RoundToInt(distance)));
                        currentCuffed++;
                    }

                    if (BetterScp049Integration.IsActive && BetterScp049Integration.Disarmed049.TryGetValue(cuffer, out var scp))
                    {
                        try
                        {
                            // var distance = Vector3.Distance(cuffer.Position, scp.Position);
                            // cuffed.Add(string.Format(PluginHandler.Instance.Translation.CufferElementOfListInfo, scp.GetDisplayName(), Mathf.RoundToInt(distance)));
                            currentCuffed++;
                        }
                        catch
                        {
                        }
                    }

                    var limit = GetCuffingLimit(cuffer);

                    while (currentCuffed > limit)
                    {
                        foreach (Player target in RealPlayers.List.ToArray())
                        {
                            if (target.IsAlive && target.IsCuffed && target.Cuffer == cuffer)
                            {
                                currentCuffed--;
                                target.RemoveHandcuffs();
                                break;
                            }
                            else if (BetterScp049Integration.IsActive && BetterScp049Integration.Disarmed049.ContainsKey(cuffer))
                            {
                                BetterScp049Integration.Disarmed049.Remove(cuffer);
                            }
                        }
                    }

                    if (cuffed.Count != 0)
                    {
                        cuffer.SetGUI($"cuffer-{cuffer.Nickname}", PseudoGUIPosition.BOTTOM, string.Format(PluginHandler.Instance.Translation.CufferListOfTargetsInfo, currentCuffed, limit, string.Join("<br>", cuffed)));
                    }
                    else
                    {
                        cuffer.SetGUI($"cuffer-{cuffer.Nickname}", PseudoGUIPosition.BOTTOM, null);
                        break;
                    }
                }
                catch (System.Exception ex)
                {
                    Instance.Log.Error(ex);
                }

                yield return Timing.WaitForSeconds(1);
            }

            cuffer.SetGUI($"cuffer-{cuffer.Nickname}", PseudoGUIPosition.BOTTOM, null);
        }

        public static IEnumerator<float> CuffedPlayerInfo(Player target)
        {
            yield return Timing.WaitForSeconds(1);

            while (target.IsConnected())
            {
                if (target.IsCuffed)
                {
                    Mistaken.API.Handlers.CustomInfoHandler.Set(target, $"cuffed-{target.Nickname}", string.Format(PluginHandler.Instance.Translation.CuffedBy, target.Cuffer.GetDisplayName()));
                }
                else if (BetterScp049Integration.IsActive && BetterScp049Integration.Disarmed049.ContainsValue(target))
                {
                    try
                    {
                        Mistaken.API.Handlers.CustomInfoHandler.Set(
                            target,
                            $"cuffed-{target.Nickname}",
                            string.Format(
                                PluginHandler.Instance.Translation.CuffedBy,
                                BetterScp049Integration.Disarmed049.First(x => x.Value == target).Key.GetDisplayName()));
                    }
                    catch
                    {
                    }
                }
                else
                {
                    Mistaken.API.Handlers.CustomInfoHandler.Set(target, $"cuffed-{target.Nickname}", string.Empty);
                    break;
                }

                yield return Timing.WaitForSeconds(1);
            }
        }

        public CustomCuffingHandler(PluginHandler plugin)
            : base(plugin)
        {
            Instance = this;
        }

        public override string Name => nameof(CustomCuffingHandler);

        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.Handcuffing += this.Player_Handcuffing;
            Exiled.Events.Handlers.Player.Hurting += this.Player_Hurting;
            Events.Handlers.CustomEvents.Uncuffing += this.Player_Uncuffing;
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Handcuffing -= this.Player_Handcuffing;
            Exiled.Events.Handlers.Player.Hurting -= this.Player_Hurting;
            Events.Handlers.CustomEvents.Uncuffing -= this.Player_Uncuffing;
        }

        internal static CustomCuffingHandler Instance { get; private set; }

        private static IEnumerable<Player> GetCuffedPlayers(Player cuffer)
            => RealPlayers.List.Where(x => x.IsAlive && x.Cuffer == cuffer);

        private static ushort GetCuffingLimit(Player cuffer)
        {
            ushort limit = 0;

            if (cuffer.HasItem(ItemType.ArmorLight))
                limit = 1;
            else if (cuffer.HasItem(ItemType.ArmorCombat))
                limit = 2;
            else if (cuffer.HasItem(ItemType.ArmorHeavy))
                limit = 4;

            return limit;
        }

        private void Player_Handcuffing(HandcuffingEventArgs ev)
        {
            int currentCuffed = GetCuffedPlayers(ev.Cuffer).Count();

            if (BetterScp049Integration.IsActive && BetterScp049Integration.Disarmed049.ContainsKey(ev.Cuffer))
                currentCuffed += 1;

            var limit = GetCuffingLimit(ev.Cuffer);

            if (currentCuffed >= limit)
            {
                ev.IsAllowed = false;
                ev.Cuffer.SetGUI($"cuffer-{ev.Cuffer.Nickname}", PseudoGUIPosition.MIDDLE, string.Format(PluginHandler.Instance.Translation.CuffingLimitInfo, limit), 5);
                return;
            }

            this.Log.Debug($"Cuffer: {ev.Cuffer.ReferenceHub.playerMovementSync.PlayerVelocity} ({ev.Cuffer.ReferenceHub.playerMovementSync.PlayerVelocity != Vector3.zero})", PluginHandler.Instance.Config.VerboseOutput);
            this.Log.Debug($"Target: {ev.Target.ReferenceHub.playerMovementSync.PlayerVelocity} ({ev.Target.ReferenceHub.playerMovementSync.PlayerVelocity != Vector3.zero})", PluginHandler.Instance.Config.VerboseOutput);

            if (ev.Cuffer.ReferenceHub.playerMovementSync.PlayerVelocity != Vector3.zero)
            {
                ev.IsAllowed = false;
                ev.Cuffer.SetGUI($"cuffer-{ev.Cuffer.Nickname}", PseudoGUIPosition.MIDDLE, PluginHandler.Instance.Translation.CufferMovingWhenCuffingInfo, 5);
                this.Log.Debug("MOVING CUFFER, NOT GOOD :/", PluginHandler.Instance.Config.VerboseOutput);
                return;
            }

            if (ev.Target.ReferenceHub.playerMovementSync.PlayerVelocity != Vector3.zero)
            {
                ev.IsAllowed = false;
                ev.Cuffer.SetGUI($"cuffer-{ev.Cuffer.Nickname}", PseudoGUIPosition.MIDDLE, PluginHandler.Instance.Translation.TargetMovingWhenCuffingInfo, 5);
                this.Log.Debug("MOVING TARGET, NOT GOOD :/", PluginHandler.Instance.Config.VerboseOutput);
            }

            if (currentCuffed == 0)
                this.RunCoroutine(CufferGUI(ev.Cuffer), nameof(CufferGUI), true);

            this.RunCoroutine(CuffedPlayerInfo(ev.Target), nameof(CuffedPlayerInfo), true);
        }

        private void Player_Hurting(HurtingEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;

            if (ev.Attacker != null && ev.Target != null)
            {
                var cuffer = ev.Target.Cuffer;

                if (cuffer == ev.Attacker)
                    return;

                if (cuffer != null && cuffer.Role.Side == ev.Attacker.Role.Side)
                {
                    ev.Attacker.Broadcast(5, string.Format(PluginHandler.Instance.Translation.AttackingCuffedInfo, ev.Target.GetDisplayName(), cuffer.GetDisplayName()), shouldClearPrevious: true);
                    ev.Target.Broadcast(5, string.Format(PluginHandler.Instance.Translation.CuffedAttackedInfo, ev.Attacker.GetDisplayName()), shouldClearPrevious: true);
                }
                else if (BetterScp049Integration.Disarmed049.ContainsValue(ev.Target))
                {
                    var kvp = BetterScp049Integration.Disarmed049.First(x => x.Value == ev.Target);

                    if (kvp.Key == ev.Attacker)
                        return;

                    if (kvp.Key.Role.Side == ev.Attacker.Role.Side)
                    {
                        ev.Attacker.Broadcast(5, string.Format(PluginHandler.Instance.Translation.AttackingCuffedInfo, ev.Target.GetDisplayName(), kvp.Key.GetDisplayName()), shouldClearPrevious: true);
                        ev.Target.Broadcast(5, string.Format(PluginHandler.Instance.Translation.CuffedAttackedInfo, ev.Attacker.GetDisplayName()), shouldClearPrevious: true);
                    }
                }
            }
        }

        private void Player_Uncuffing(Events.EventArgs.UncuffingEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;

            if (ev.UnCuffer.IsScp)
                ev.IsAllowed = PluginHandler.Instance.Config.AllowScps;

            if (ev.Target.Cuffer.IsNTF && (ev.UnCuffer?.IsNTF ?? false) && ev.UnCuffer != ev.Target.Cuffer)
                ev.IsAllowed = PluginHandler.Instance.Config.AllowOtherMtfs;
        }
    }
}
