// -----------------------------------------------------------------------
// <copyright file="ValidateEntryPatch.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Reflection.Emit;
using Exiled.API.Features;
using HarmonyLib;
using InventorySystem.Disarming;
using UnityEngine;

namespace Mistaken.CustomCuffing
{
    [HarmonyPatch(typeof(DisarmedPlayers), nameof(DisarmedPlayers.ValidateEntry))]
    internal static class ValidateEntryPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Rent(instructions);

            int index = newInstructions.FindIndex(x => x.opcode == OpCodes.Ldc_R4);
            float squared = Mathf.Pow(PluginHandler.Instance.Config.AutoDisarmDistance, 2f);

            newInstructions.RemoveAt(index);
            newInstructions.Insert(index, new(OpCodes.Ldc_R4, squared));

            for (int i = 0; i < newInstructions.Count; i++)
                yield return newInstructions[i];

            NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}
