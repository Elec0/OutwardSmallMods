using System.Reflection;
using UnityEngine;
using OModAPI;

namespace DodgeOverhaul
{
    public class DodgeBehavior : MonoBehaviour
    {
        public static DodgeOverhaulMod mod;

        MethodInfo hasEnoughStamina;
        FieldInfo dodging;
        FieldInfo characterSoundManager;
        FieldInfo dodgeSoundPlayer;

        public void Initialize()
        {
            hasEnoughStamina = ReflectionTools.GetMethod(typeof(Character), "HasEnoughStamina");
            dodging = ReflectionTools.GetField(typeof(Character), "m_dodging");
            characterSoundManager = ReflectionTools.GetField(typeof(Character), "m_characterSoundManager");
            dodgeSoundPlayer = ReflectionTools.GetField(typeof(Character), "m_dodgeSoundPlayer");

            Patch();
        }

        public void Patch()
        {
            On.Character.DodgeInput_1 += new On.Character.hook_DodgeInput_1(dodgeInput);
            On.Character.SendDodgeTriggerTrivial += new On.Character.hook_SendDodgeTriggerTrivial(dodgeTrigger);
            On.Character.ResetDodgeTrigger += new On.Character.hook_ResetDodgeTrigger(resetDodgeTrigger);
        }

        public void dodgeInput(On.Character.orig_DodgeInput_1 orig, Character self, Vector3 _direction)
        {            
            FieldInfo dodgeAllowed = ReflectionTools.GetField(typeof(Character), "m_dodgeAllowedInAction");

            if (!self.IsPhotonPlayerLocal || (double)self.Stats.MovementSpeed <= 0.0 || (self.PreparingToSleep || !(bool)hasEnoughStamina.Invoke(self, new object[] { 6f })) ||
                self.LocomotionAction && !self.CurrentlyChargingAttack || (!self.InLocomotion || !self.NextIsLocomotion) && (!self.NextIsLocomotion && 
                 (int)dodgeAllowed.GetValue(self) <= 0))
                return;
            dodgeAllowed.SetValue(self, 0);

            if ((bool)((Object)self.CharacterCamera) && self.CharacterCamera.InZoomMode)
                self.SetZoomMode(false);
            if (self.CurrentSpellCast != Character.SpellCastType.NONE)
            {
                if (self.CurrentSpellCast == Character.SpellCastType.PickupBagGround || self.CurrentSpellCast == Character.SpellCastType.DropBagGround)
                    self.ForceCancel(false, true);
                self.ResetCastType();
            }
            self.photonView.RPC("SendDodgeTriggerTrivial", PhotonTargets.All, (object)_direction);
            ReflectionTools.GetMethod(typeof(Character), "ActionPerformed").Invoke(self, new object[] { true });

            self.Invoke("ResetDodgeTrigger", 0.5f);
        }

        protected void resetDodgeTrigger(On.Character.orig_ResetDodgeTrigger orig, Character self)
        {
            self.Animator.ResetTrigger("Dodge");
        }

        [PunRPC]
        public void dodgeTrigger(On.Character.orig_SendDodgeTriggerTrivial orig, Character self, Vector3 _direction)
        {
            // The main dodge slowness aspect
            if (self.HasDodgeDirection)
                self.Animator.SetFloat("DodgeBlend", !self.DodgeRestricted ? 0.0f : getDodgeRestrictedAmount(self));

            self.Animator.SetTrigger("Dodge");

            if (self.CurrentlyChargingAttack)
                ReflectionTools.GetMethod(typeof(Character), "SendCancelCharging").Invoke(self, null);

            ((SoundPlayer)dodgeSoundPlayer.GetValue(self)).Play(false);
            dodging.SetValue(self, true);

            ReflectionTools.GetMethod(typeof(Character), "StopBlocking").Invoke(self, null);

            if (self.OnDodgeEvent != null)
                self.OnDodgeEvent();

            if (characterSoundManager.GetValue(self) != null)
                Global.AudioManager.PlaySoundAtPosition(((CharacterSoundManager)characterSoundManager.GetValue(self)).GetDodgeSound(), self.transform, 0.0f, 1f, 1f, 1f, 1f);
            self.SendMessage("DodgeTrigger", (object)_direction, SendMessageOptions.DontRequireReceiver);
        }

        private float getDodgeRestrictedAmount(Character self)
        {
            float min_dodge = 0.0f;
            float min_restricted_dodge = 0.2f;
            float min_bag_num = 0.4f; // Up to 40% of bag can be filled and not take dodge above min_restricted_dodge
            float max_dodge = 1.0f;
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

        private Bag getCharacterBag(Character self)
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
    }
}
