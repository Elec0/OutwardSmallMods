using System.Reflection;
using UnityEngine;

namespace OutwardTestMod1
{
    public class TestBehavior : MonoBehaviour
    {
        public static TestMod1 mod;

        MethodInfo hasEnoughStamina;
        FieldInfo dodging;
        FieldInfo blocking;
        FieldInfo characterSoundManager;

        public void Initialize()
        {
            hasEnoughStamina = ReflectionTools.GetMethod(typeof(Character), "HasEnoughStamina");
            dodging = ReflectionTools.GetField(typeof(Character), "m_dodging");
            characterSoundManager = ReflectionTools.GetField(typeof(Character), "m_characterSoundManager");

            Patch();
        }

        public void Patch()
        {
            On.Character.DodgeInput_1 += new On.Character.hook_DodgeInput_1(dodgeInput);
            On.Character.SendDodgeTriggerTrivial += new On.Character.hook_SendDodgeTriggerTrivial(dodgeTrigger);
        }


        public void dodgeInput(On.Character.orig_DodgeInput_1 orig, Character self, Vector3 _direction)
        {
            //PlayerLifeStory lifeStory = typeof(BasePlayer).GetField("lifeStory", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player) as PlayerLifeStory;
            if (!self.IsPhotonPlayerLocal || (double)self.Stats.MovementSpeed <= 0.0 || (self.PreparingToSleep || !(bool)hasEnoughStamina.Invoke(self, new object[] { 6f })) ||
                self.LocomotionAction && !self.CurrentlyChargingAttack || (!self.InLocomotion || !self.NextIsLocomotion) /*&& (!self.NextIsLocomotion && 
                self.m_dodgeAllowedInAction <= 0)*/)
                return;
            //self.m_dodgeAllowedInAction = 0;
            if ((bool)((Object)self.CharacterCamera) && self.CharacterCamera.InZoomMode)
                self.SetZoomMode(false);
            if (self.CurrentSpellCast != Character.SpellCastType.NONE)
            {
                if (self.CurrentSpellCast == Character.SpellCastType.PickupBagGround || self.CurrentSpellCast == Character.SpellCastType.DropBagGround)
                    self.ForceCancel(false, true);
                self.ResetCastType();
            }
            self.photonView.RPC("SendDodgeTriggerTrivial", PhotonTargets.All, (object)_direction);
            //self.ActionPerformed(true);

            self.Invoke("ResetDodgeTrigger", 0.1f);
        }

        [PunRPC]
        public void dodgeTrigger(On.Character.orig_SendDodgeTriggerTrivial orig, Character self, Vector3 _direction)
        {
            if (self.HasDodgeDirection)
                self.Animator.SetFloat("DodgeBlend", !self.DodgeRestricted ? 0.0f : 0.0f);
            self.Animator.SetTrigger("Dodge");
            if (self.CurrentlyChargingAttack)
                ReflectionTools.GetMethod(typeof(Character), "SendCancelCharging").Invoke(self, null);
            //self.sound.Play(false);
            dodging.SetValue(self, true);

            ReflectionTools.GetMethod(typeof(Character), "StopBlocking").Invoke(self, null);

            if (self.OnDodgeEvent != null)
                self.OnDodgeEvent();

            if (characterSoundManager.GetValue(self) != null)
                Global.AudioManager.PlaySoundAtPosition(((CharacterSoundManager)characterSoundManager.GetValue(self)).GetDodgeSound(), self.transform, 0.0f, 1f, 1f, 1f, 1f);
            self.SendMessage("DodgeTrigger", (object)_direction, SendMessageOptions.DontRequireReceiver);
        }
    }
}
