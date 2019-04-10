using UnityEngine;
using OModAPI;
using On;
using System;

namespace SneakAttack
{
    public class SneakAttackBehavior: MonoBehaviour
    {
        public bool effectPlayer = false;
        public float sneakMult = 2.0f;

        public void Initialize()
        {
            ConfigHelper configHelper = new ConfigHelper(ConfigHelper.ConfigModes.CreateIfMissing, "SneakAttack.xml");
            configHelper.XMLDefaultConfig = "<sneakAttack><sneakMultiplier>2.0</sneakMultiplier><effectPlayer>false</effectPlayer></sneakAttack>";

            bool.TryParse(configHelper.ReadString("/sneakAttack/effectPlayer"), out effectPlayer);
            try
            {
                sneakMult = configHelper.ReadFloat("/sneakAttack/sneakMultiplier");
            }
            catch(FormatException e)
            {
                Debug.Log(e.StackTrace);
            }

            Patch();
        }

        public void Patch()
        {
            On.Character.ReceiveHit += new On.Character.hook_ReceiveHit(receiveHit);
        }

        private DamageList receiveHit(On.Character.orig_ReceiveHit orig, Character self, Weapon _weapon, DamageList _damage, Vector3 _hitDir, Vector3 _hitPoint, float _angle, float _angleDir, Character _dealerChar, float _knockBack, bool _hitInventory)
        {
            DamageList changedDamages = null;
            if(!self.TargetingSystem.Locked)
            {
                if(self.IsAI || (!self.IsAI && effectPlayer))
                {
                    changedDamages = _damage.Clone();
                    for(int i = 0; i < changedDamages.Count; ++i)
                    {
                        changedDamages[i].Damage *= sneakMult;
                    }
                }
            }
            return orig(self, _weapon, changedDamages ?? _damage, _hitDir, _hitPoint, _angle, _angleDir, _dealerChar, _knockBack, _hitInventory);
        }
    }
}
