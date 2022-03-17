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
            Instance = this;
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
            int currentCuffed = GetCuffedPlayers(ev.Cuffer).Count();
            if (BetterScp049Integration.IsActive && BetterScp049Integration.Disarmed049.ContainsKey(ev.Cuffer))
                currentCuffed += 1;

            var limit = GetCuffingLimit(ev.Cuffer);
            if (currentCuffed >= limit)
            {
                ev.IsAllowed = false;
                ev.Cuffer.SetGUI($"cuffer-{ev.Cuffer.Nickname}", PseudoGUIPosition.MIDDLE, $"<color=red>You can't cuff someone! (You can cuff max {limit} people)</color>", 5);
                return;
            }

            this.Log.Debug($"Cuffer: {ev.Cuffer.ReferenceHub.playerMovementSync.PlayerVelocity} ({ev.Cuffer.ReferenceHub.playerMovementSync.PlayerVelocity != Vector3.zero})", PluginHandler.Instance.Config.VerbouseOutput);
            this.Log.Debug($"Target: {ev.Target.ReferenceHub.playerMovementSync.PlayerVelocity} ({ev.Target.ReferenceHub.playerMovementSync.PlayerVelocity != Vector3.zero})", PluginHandler.Instance.Config.VerbouseOutput);
            if (ev.Cuffer.ReferenceHub.playerMovementSync.PlayerVelocity != Vector3.zero)
            {
                ev.IsAllowed = false;
                ev.Cuffer.SetGUI($"cuffer-{ev.Cuffer.Nickname}", PseudoGUIPosition.MIDDLE, $"<color=red>You can't cuff someone! (You're moving)</color>", 5);
                this.Log.Debug("MOVING CUFFER, NOT GOOD :/", PluginHandler.Instance.Config.VerbouseOutput);
                return;
            }

            if (ev.Target.ReferenceHub.playerMovementSync.PlayerVelocity != Vector3.zero)
            {
                ev.IsAllowed = false;
                ev.Cuffer.SetGUI($"cuffer-{ev.Cuffer.Nickname}", PseudoGUIPosition.MIDDLE, $"<color=red>You can't cuff someone! (Your Target is moving)</color>", 5);
                this.Log.Debug("MOVING TARGET, NOT GOOD :/", PluginHandler.Instance.Config.VerbouseOutput);
            }

            if (currentCuffed == 0)
            {
                Timing.RunCoroutine(CufferGUI(ev.Cuffer));
            }

            Timing.RunCoroutine(CuffedPlayerInfo(ev.Target));
        }

        private void Player_Uncuffing(Events.EventArgs.UncuffingEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;

            if (ev.UnCuffer.IsScp)
            {
                ev.IsAllowed = PluginHandler.Instance.Config.AllowScps;
            }

            if (ev.Target.Cuffer.IsNTF && (ev.UnCuffer?.IsNTF ?? false) && ev.UnCuffer != ev.Target.Cuffer)
            {
                ev.IsAllowed = PluginHandler.Instance.Config.AllowOtherMtfs;
            }
        }

        public static IEnumerator<float> CufferGUI(Player cuffer)
        {
            yield return Timing.WaitForSeconds(1);
            while (cuffer.IsConnected && cuffer.IsAlive)
            {
                try
                {
                    List<string> cuffed = new List<string>();
                    int currentCuffed = 0;
                    foreach (Player target in GetCuffedPlayers(cuffer))
                    {
                        var distance = Vector3.Distance(cuffer.Position, target.Position);
                        cuffed.Add($"<color=yellow>{target.Nickname}</color> - <color=yellow>{Mathf.RoundToInt(distance)}</color>m away");
                        currentCuffed++;
                    }

                    if (BetterScp049Integration.IsActive)
                    {
                        if (BetterScp049Integration.Disarmed049.TryGetValue(cuffer, out var scp))
                        {
                            var distance = Vector3.Distance(cuffer.Position, scp.Position);
                            cuffed.Add($"<color=yellow>{scp.Nickname}</color> - <color=yellow>{Mathf.RoundToInt(distance)}</color>m away");
                            currentCuffed++;
                        }
                    }

                    var limit = GetCuffingLimit(cuffer);
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
                            else if (BetterScp049Integration.IsActive && BetterScp049Integration.Disarmed049.ContainsKey(cuffer))
                                BetterScp049Integration.Disarmed049.Remove(cuffer);
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
                    Instance.Log.Error(ex);
                }

                yield return Timing.WaitForSeconds(1);
            }

            cuffer.SetGUI($"cuffer-{cuffer.Nickname}", PseudoGUIPosition.BOTTOM, null);
        }

        public static IEnumerator<float> CuffedPlayerInfo(Player target)
        {
            yield return Timing.WaitForSeconds(1);
            while (target.IsConnected && target.IsAlive)
            {
                if (target.IsCuffed || (BetterScp049Integration.IsActive && BetterScp049Integration.Disarmed049.ContainsValue(target)))
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

        internal static CustomCuffingHandler Instance;

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
    }
}
