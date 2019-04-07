using System;
using System.IO;
using System.Reflection;
using System.Xml;
using UnityEngine;

namespace StealthSpeedIncrease
{
    public class StealthSpeedIncreaseBehavior: MonoBehaviour
    {
        // Reflected stuff
        FieldInfo m_autoRun         = ReflectionTools.GetField(typeof(LocalCharacterControl), "m_autoRun");
        FieldInfo m_sprintTime      = ReflectionTools.GetField(typeof(LocalCharacterControl), "m_sprintTime");
        FieldInfo m_sprintFacing    = ReflectionTools.GetField(typeof(LocalCharacterControl), "m_sprintFacing");
        FieldInfo m_moveInput       = ReflectionTools.GetField(typeof(LocalCharacterControl), "m_moveInput");
        FieldInfo m_modifMoveInput  = ReflectionTools.GetField(typeof(LocalCharacterControl), "m_modifMoveInput");
        FieldInfo m_horiControl     = ReflectionTools.GetField(typeof(LocalCharacterControl), "m_horiControl");

        MethodInfo StopAutoRun      = ReflectionTools.GetMethod(typeof(LocalCharacterControl), "StopAutoRun");


        float baseSneakSpeed = 0.7f;
        float stealthTrainingBonus = 1.3f;

        public void Initialize()
        {
            // Read config file
            ConfigHelper configHelper = new ConfigHelper("StealthSpeedConfig.xml");
            configHelper.Mode = ConfigHelper.ConfigModes.CreateIfMissing;

            Debug.Log("Trying to load " + configHelper.FullPath);

            baseSneakSpeed = configHelper.ReadFloat("/config/baseSneakSpeed");
            stealthTrainingBonus = configHelper.ReadFloat("/config/stealthTrainingBonus");
            Debug.Log("Successfully loaded.");            
            
            Patch();
        }

        private void Patch()
        {
            On.LocalCharacterControl.DetectMovementInputs += new On.LocalCharacterControl.hook_DetectMovementInputs(detectMovementInputs);
        }

        public void detectMovementInputs(On.LocalCharacterControl.orig_DetectMovementInputs orig, LocalCharacterControl self)
        {
            Type tSelf = self.GetType();

            // Remove the need for reflection as much as possible
            // These need to be manually set
            Vector2 sm_moveInput;
            Vector3 sm_modifMoveInput;

            // This should update anything changed inside of it
            Character m_character = self.Character;

            if (self.InputLocked)
            {
                sm_moveInput = Vector2.zero;
                sm_modifMoveInput = (Vector3)Vector2.zero;
                // Exit condition, so set stuff
                m_moveInput.SetValue(self, sm_moveInput);
                m_modifMoveInput.SetValue(self, sm_modifMoveInput);

                if (!m_character.Sprinting)
                    return;
                m_character.SprintInput(false);
            }
            else
            {
                // Static methods are great
                sm_moveInput.x = ControlsInput.MoveHorizontal(m_character.OwnerPlayerSys.PlayerID);
                sm_moveInput.y = ControlsInput.MoveVertical(m_character.OwnerPlayerSys.PlayerID);

                // If we're autorunning
                if ((bool)m_autoRun.GetValue(self))
                {
                    if ((double)sm_moveInput.y == 0.0)
                        sm_moveInput.y = 1f;
                    else
                        StopAutoRun.Invoke(self, null);
                }

                // Make sure the magnitude of our input vector is at max 1
                sm_moveInput = (Vector2)Vector3.ClampMagnitude((Vector3)sm_moveInput, 1f);
                // Switch to 3D vector
                sm_modifMoveInput = (Vector3)sm_moveInput;

                // Sprinting
                if (ControlsInput.Sprint(m_character.OwnerPlayerSys.PlayerID) && m_character.InLocomotion && m_character.Stats.CanSprint() && ((!m_character.LocomotionAction || (double)m_character.MobileCastMoveMult == -1.0) && (double)sm_moveInput.sqrMagnitude > 0.100000001490116))
                {
                    // We can cast this because m_sprintTime is always a float
                    m_sprintTime.SetValue(self, (float)m_sprintTime.GetValue(self) + Time.deltaTime);

                    if (m_character.Sneaking)
                        m_character.StealthInput(false);
                    if (m_character.CurrentlyChargingAttack && !m_character.CancelChargingSent)
                        m_character.CancelCharging();

                    // I think this normalize call is pointless
                    sm_modifMoveInput.Normalize();
                    sm_modifMoveInput *= m_character.Speed * 1.75f;
                    m_sprintFacing.SetValue(self, true);
                    m_character.SprintInput(true);

                    if (m_character.BlockDesired)
                        m_character.BlockInput(false);
                }
                else // Not sprinting
                {
                    m_sprintTime.SetValue(self, 0.0f);

                    // Do the calculations for our speed normally, then apply modifiers
                    // Having your weapon unsheathed makes you 12.5% slower
                    sm_modifMoveInput *= !m_character.Sheathed ? m_character.Speed * 0.875f : m_character.Speed;

                    // Modify speed based on sneaking status
                    if (m_character.Sneaking)
                    {
                        // Vanilla values have movement speed capped at 2.3 while sneaking, with a 30% increase with the skill
                        float sneakMod = baseSneakSpeed;

                        // Handle "Stealth Training" skill
                        if (m_character.Inventory.SkillKnowledge.IsItemLearned(8205190))
                            sneakMod *= stealthTrainingBonus;

                        sm_modifMoveInput *= sneakMod;
                    }


                    if (m_character.Sprinting)
                        m_character.SprintInput(false);
                    if (m_character.Blocking || m_character.CurrentlyChargingAttack)
                        sm_modifMoveInput *= 0.6f;
                    if (!m_character.LocomotionAllowed)
                        sm_modifMoveInput *= 0.1f;
                    if (m_character.LocomotionAction && (double)m_character.MobileCastMoveMult > 0.0)
                        sm_modifMoveInput *= m_character.MobileCastMoveMultSmooth;
                    if (m_character.Sliding)
                        sm_modifMoveInput *= 0.6f;
                    if (m_character.Falling)
                        sm_modifMoveInput *= 0.3f;

                    if (self.FaceLikeCamera)
                    {
                        Vector2 modifMoveInput = (Vector2)sm_modifMoveInput;
                        float num = 0.1f;
                        sm_modifMoveInput.y *= (float)(1.0 + (double)Mathf.Abs(modifMoveInput.x) * (double)num);
                        sm_modifMoveInput.x *= (float)(1.0 + (double)Mathf.Abs(modifMoveInput.y) * (double)num);
                    }
                    m_sprintFacing.SetValue(self, false);

                }
                // General movement modifiers
                sm_modifMoveInput *= self.MovementMultiplier * m_character.Stats.MovementSpeed;
                
                if (m_character.PreparingToSleep)
                    sm_modifMoveInput *= 0.0f;
                
                // There is an exit condition ahead, so set values
                m_modifMoveInput.SetValue(self, sm_modifMoveInput);
                m_moveInput.SetValue(self, sm_moveInput);

                if ((m_character.CharacterUI == null || !m_character.CharacterUI.IsMenuJustToggled) && ControlsInput.DodgeButton(m_character.OwnerPlayerSys.PlayerID))
                {
                    Transform horiControl = (Transform)m_horiControl.GetValue(self);

                    m_character.DodgeInput(horiControl.forward * sm_moveInput.y + horiControl.right * sm_moveInput.x);
                    StopAutoRun.Invoke(self, null);
                }
                if (ControlsInput.AutoRun(m_character.OwnerPlayerSys.PlayerID))
                    m_autoRun.SetValue(self, !(bool)m_autoRun.GetValue(self));

                if (!((bool)m_character.CharacterUI) || m_character.CharacterUI.IsMenuJustToggled || (ControlsInput.QuickSlotToggled(m_character.OwnerPlayerSys.PlayerID) || !ControlsInput.StealthButton(m_character.OwnerPlayerSys.PlayerID)))
                    return;

                m_character.StealthInput(!m_character.Sneaking);
            }
        }
    }
}
