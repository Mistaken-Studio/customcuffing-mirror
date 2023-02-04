using System.Collections.Generic;
using System.Linq;
using MEC;
using Mistaken.API.Extensions;
using Mistaken.PseudoGUI;
using PlayerRoles;
using PlayerStatsSystem;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using UnityEngine;

namespace Mistaken.CustomCuffing;

internal sealed class CustomCuffingHandler
{
    public static IEnumerator<float> CufferGUI(Player cuffer)
    {
        yield return Timing.WaitForSeconds(1);

        while (cuffer.IsConnected() && cuffer.IsAlive)
        {
            try
            {
                List<string> cuffed = new();
                int currentCuffed = 0;

                foreach (Player target in GetCuffedPlayers(cuffer))
                {
                    // var distance = Vector3.Distance(cuffer.Position, target.Position);
                    // cuffed.Add(string.Format(PluginHandler.Instance.Translation.CufferElementOfListInfo, target.GetDisplayName(), Mathf.RoundToInt(distance)));
                    cuffed.Add($"<color=yellow>{target.GetDisplayName()}</color>");
                    currentCuffed++;
                }

                /*if (BetterScp049Integration.IsActive && BetterScp049Integration.Disarmed049.TryGetValue(cuffer, out var scp))
                {
                    try
                    {
                        // var distance = Vector3.Distance(cuffer.Position, scp.Position);
                        // cuffed.Add(string.Format(PluginHandler.Instance.Translation.CufferElementOfListInfo, scp.GetDisplayName(), Mathf.RoundToInt(distance)));
                        cuffed.Add($"<color=yellow>{scp.GetDisplayName()}</color>");
                        currentCuffed++;
                    }
                    catch
                    {
                    }
                }*/

                var limit = GetCuffingLimit(cuffer);

                while (currentCuffed > limit)
                {
                    foreach (Player target in Player.GetPlayers())
                    {
                        if (target.IsAlive && target.IsDisarmed && target.DisarmedBy == cuffer)
                        {
                            currentCuffed--;
                            target.IsDisarmed = false;
                            break;
                        }
                        /*else if (BetterScp049Integration.IsActive && BetterScp049Integration.Disarmed049.ContainsKey(cuffer))
                            BetterScp049Integration.Disarmed049.Remove(cuffer);*/
                    }
                }

                if (cuffed.Count != 0)
                {
                    cuffer.SetGUI($"cuffer-{cuffer.Nickname}", PseudoGUIPosition.BOTTOM, string.Format(Plugin.Translations.CufferListOfTargetsInfo, currentCuffed, limit, string.Join("<br>", cuffed)));
                }
                else
                {
                    cuffer.SetGUI($"cuffer-{cuffer.Nickname}", PseudoGUIPosition.BOTTOM, null);
                    break;
                }
            }
            catch (System.Exception ex)
            {
                Log.Error(ex.ToString());
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
            if (target.IsDisarmed)
                target.SetCustomInfo($"cuffed-{target.Nickname}", string.Format(Plugin.Translations.CuffedBy, target.DisarmedBy.GetDisplayName()));
            /*else if (BetterScp049Integration.IsActive && BetterScp049Integration.Disarmed049.ContainsValue(target))
            {
                try
                {
                    target.SetCustomInfo(
                        $"cuffed-{target.Nickname}",
                        string.Format(
                            Plugin.Translations.CuffedBy,
                            BetterScp049Integration.Disarmed049.First(x => x.Value == target).Key.GetDisplayName()));
                }
                catch
                {
                }
            }*/
            else
            {
                target.SetCustomInfo($"cuffed-{target.Nickname}", string.Empty);
                break;
            }

            yield return Timing.WaitForSeconds(1);
        }
    }

    public CustomCuffingHandler()
    {
        EventManager.RegisterEvents(this);
    }

    ~CustomCuffingHandler()
    {
        EventManager.UnregisterEvents(this);
    }

    private static IEnumerable<Player> GetCuffedPlayers(Player cuffer)
        => Player.GetPlayers().Where(x => x.IsAlive && x.DisarmedBy == cuffer);

    private static ushort GetCuffingLimit(Player cuffer)
    {
        ushort limit = 0;

        if (cuffer.Items.Any(x => x.ItemTypeId == ItemType.ArmorLight))
            limit = 2;
        else if (cuffer.Items.Any(x => x.ItemTypeId == ItemType.ArmorCombat))
            limit = 2;
        else if (cuffer.Items.Any(x => x.ItemTypeId == ItemType.ArmorHeavy))
            limit = 4;

        return limit;
    }

    [PluginEvent(ServerEventType.PlayerHandcuff)]
    private bool OnPlayerHandcuff(Player player, Player target)
    {
        if (player is null)
            return true;

        int currentCuffed = GetCuffedPlayers(player).Count();

        /*if (BetterScp049Integration.IsActive && BetterScp049Integration.Disarmed049.ContainsKey(player))
            currentCuffed += 1;*/

        var limit = GetCuffingLimit(player);

        if (currentCuffed >= limit)
        {
            player.SetGUI($"cuffer-{player.Nickname}", PseudoGUIPosition.MIDDLE, string.Format(Plugin.Translations.CuffingLimitInfo, limit), 5);
            return false;
        }

        Log.Debug($"Cuffer: {player.Velocity} ({player.Velocity != Vector3.zero})", Plugin.Instance.Config.Debug);
        Log.Debug($"Target: {target.Velocity} ({target.Velocity != Vector3.zero})", Plugin.Instance.Config.Debug);

        if (player.Velocity != Vector3.zero)
        {
            player.SetGUI($"cuffer-{player.Nickname}", PseudoGUIPosition.MIDDLE, Plugin.Translations.CufferMovingWhenCuffingInfo, 5);
            Log.Debug("MOVING CUFFER, NOT GOOD :/", Plugin.Instance.Config.Debug);
            return false;
        }

        if (target.Velocity != Vector3.zero)
        {
            player.SetGUI($"cuffer-{player.Nickname}", PseudoGUIPosition.MIDDLE, Plugin.Translations.TargetMovingWhenCuffingInfo, 5);
            Log.Debug("MOVING TARGET, NOT GOOD :/", Plugin.Instance.Config.Debug);
            return false;
        }

        if (currentCuffed == 0)
            Timing.RunCoroutine(CufferGUI(player));

        Timing.RunCoroutine(CuffedPlayerInfo(target));
        return true;
    }

    [PluginEvent(ServerEventType.PlayerDamage)]
    private void OnPlayerDamage(Player target, Player attacker, DamageHandlerBase handler)
    {
        if (attacker is null || target is null)
            return;

        var cuffer = target.DisarmedBy;
        if (cuffer == attacker)
            return;

        if (cuffer is not null && cuffer.ReferenceHub.GetFaction() == attacker.ReferenceHub.GetFaction())
        {
            attacker.SendBroadcast(string.Format(Plugin.Translations.AttackingCuffedInfo, target.GetDisplayName(), cuffer.GetDisplayName()), 5, shouldClearPrevious: true);
            target.SendBroadcast(string.Format(Plugin.Translations.CuffedAttackedInfo, attacker.GetDisplayName()), 5, shouldClearPrevious: true);
        }
        /*else if (BetterScp049Integration.Disarmed049.ContainsValue(target))
        {
            var kvp = BetterScp049Integration.Disarmed049.First(x => x.Value == target);

            if (kvp.Key == attacker)
                return;

            if (kvp.Key.ReferenceHub.GetFaction() == attacker.ReferenceHub.GetFaction())
            {
                attacker.SendBroadcast(string.Format(Plugin.Translations.AttackingCuffedInfo, target.GetDisplayName(), kvp.Key.GetDisplayName()), 5, shouldClearPrevious: true);
                target.SendBroadcast(string.Format(Plugin.Translations.CuffedAttackedInfo, attacker.GetDisplayName()), 5, shouldClearPrevious: true);
            }
        }*/
    }

    [PluginEvent(ServerEventType.PlayerRemoveHandcuffs)]
    private bool OnPlayerRemoveHandcuffs(Player player, Player target)
    {
        if (player is null)
            return true;

        if (player.IsSCP)
            return Plugin.Instance.Config.AllowScps;

        if (target.DisarmedBy.IsNTF && player.IsNTF && player != target.DisarmedBy)
            return Plugin.Instance.Config.AllowOtherMtfs;

        return true;
    }
}
