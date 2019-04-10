using Partiality.Modloader;
using UnityEngine;

namespace SneakAttack
{
    public class SneakAttackMod: PartialityMod
    {
        public SneakAttackMod()
        {
            this.ModID = "Sneak Attack";
            this.Version = "0001";
            this.author = "Elec0";
        }

        public static SneakAttackBehavior sneakAttackBehavior;

        public override void OnEnable()
        {
            base.OnEnable();

            GameObject obj = new GameObject();
            sneakAttackBehavior = obj.AddComponent<SneakAttackBehavior>();
            sneakAttackBehavior.Initialize();
        }
    }
}
