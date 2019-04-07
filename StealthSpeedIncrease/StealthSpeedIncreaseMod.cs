using Partiality.Modloader;
using UnityEngine;

namespace StealthSpeedIncrease
{
    public class StealthSpeedIncreaseMod: PartialityMod
    {
        public StealthSpeedIncreaseMod()
        {
            this.ModID = "Dodge Overhaul";
            this.Version = "0001";
            this.author = "Elec0";
        }

        public static StealthSpeedIncreaseBehavior stealthBehavior;

        public override void OnEnable()
        {
            base.OnEnable();

            GameObject obj = new GameObject();
            stealthBehavior = obj.AddComponent<StealthSpeedIncreaseBehavior>();
            stealthBehavior.Initialize();
        }
    }
}
