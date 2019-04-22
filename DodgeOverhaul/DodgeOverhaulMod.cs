using BepInEx;
using BepInEx.Logging;
using Harmony;
using OModAPI;
using System.Reflection;
using UnityEngine;

namespace DodgeOverhaul
{
    [BepInPlugin(packageURL, "DodgeOverhaul", "1.0.0")]
    public class DodgeOverhaulMod : BaseUnityPlugin
    {
        public const string packageURL = "com.elec0.outward.dodgeOverhaul";
        public new static ManualLogSource Logger;
        public DodgeOverhaulMod()
        {
            Logger = base.Logger;
        }

        public void Awake()
        {
            ConfigHelper configHelper = new ConfigHelper(ConfigHelper.ConfigModes.CreateIfMissing, "DodgeOverhaul.xml");
            configHelper.XMLDefaultConfig = "<dodgeOverhaul><minDodge>0.0</minDodge><minRestrictedDodge>0.2</minRestrictedDodge><minBagNum>0.4</minBagNum><maxDodge>1.0</maxDodge></dodgeOverhaul>";
            configHelper.Init();

            SendDodgeTriggerTrivial.min_dodge = configHelper.ReadFloat("/dodgeOverhaul/minDodge");
            SendDodgeTriggerTrivial.min_restricted_dodge = configHelper.ReadFloat("/dodgeOverhaul/minRestrictedDodge");
            SendDodgeTriggerTrivial.min_bag_num = configHelper.ReadFloat("/dodgeOverhaul/minBagNum");
            SendDodgeTriggerTrivial.max_dodge = configHelper.ReadFloat("/dodgeOverhaul/maxDodge");

            var harmony = HarmonyInstance.Create(packageURL);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
