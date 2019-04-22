using System;
using UnityEngine;
using Harmony;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx.Logging;
using System.Linq;

namespace DodgeOverhaul
{

    [HarmonyPatch(typeof(Character))]
    [HarmonyPatch("SendDodgeTriggerTrivial")]
    class SendDodgeTriggerTrivial
    {
        public static float min_dodge = 0.0f;
        public static float min_restricted_dodge = 0.2f;
        public static float min_bag_num = 0.4f; // Up to 40% of bag can be filled and not take dodge above min_restricted_dodge
        public static float max_dodge = 1.0f;

        static ManualLogSource Logger = DodgeOverhaulMod.Logger;

        static void Prefix(Character __instance, ref Animator ___m_animator)
        {
            if (__instance.HasDodgeDirection)
                ___m_animator.SetFloat("DodgeBlend", !__instance.DodgeRestricted ? 0.0f : getDodgeRestrictedAmount(__instance));
        }

        private static float getDodgeRestrictedAmount(Character self)
        {
            float cur_dodge = min_dodge;

            // Handle if our bag doesn't restrict us anyway
            if (!self.DodgeRestricted)
                return min_dodge;

            // Find the currently equipped bag, it should exist
            Bag bag = getCharacterBag(self);
            if (bag == null) // This shouldn't happen but who knows
                return min_dodge;

            float cap = bag.BagCapacity;
            float weight = bag.Weight;

            cur_dodge = ((Mathf.Max(weight - (min_bag_num * cap), 0) / cap) + min_restricted_dodge) * max_dodge;

            return cur_dodge;
        }

        private static Bag getCharacterBag(Character self)
        {
            EquipmentSlot[] equipmentSlots = self.Inventory.Equipment.EquipmentSlots;
            for (int i = 0; i < equipmentSlots.Length; ++i)
            {
                if (equipmentSlots[i] == null || equipmentSlots[i].EquippedItem == null)
                    continue;
                Item equippedItem = equipmentSlots[i].EquippedItem;

                if (!(equippedItem is Equipment))
                    continue;

                Equipment equipment = (Equipment)equippedItem;
                if (!(equipment is Bag))
                    continue;
                // We have found the bag
                return (Bag)equipment;
            }
            return null;
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            /* We are looking for the following two lines, and we want to skip them
            * if (this.HasDodgeDirection)
            *     this.m_animator.SetFloat("DodgeBlend", !this.DodgeRestricted ? 0.0f : 1f);
            *    
            * The compiler switches the condition into a branch-if-false, so what we're going to do is find that brfalse
            * and switch it to br, which is unconditional branch, which will skip the SetFloat method we don't want.
            * One problem with this is that brfalse consumes an item from the stack, which was loaded by the last command:
            *       IL_0001: call instance bool Character::get_HasDodgeDirection()
            * 
            * Whereas br does not consume anything. This leaves an extra value on the stack that throws an error relating to the 'ret'
            * command at the end of the method, because this method is void and shouldn't return anything. So, we need to 'pop'
            * the extra value off *before* we jump.
            */

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; ++i)
            {
                // Find the first brfalse opcode
                if (codes[i].opcode != OpCodes.Brfalse)
                    continue;

                // Ensure we have found the correct conditional
                if (codes[i - 1].operand.ToString().Contains("HasDodgeDirection"))
                {
                    // Replace the command
                    codes[i].opcode = OpCodes.Br;
                    // Insert a pop before the current command
                    codes.Insert(i, new CodeInstruction(OpCodes.Pop));
                }
                break;

            }
            return codes.AsEnumerable();
        }
    }
}

