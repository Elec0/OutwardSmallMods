using Partiality.Modloader;
using UnityEngine;

namespace OutwardTestMod1
{
    public class DodgeOverhaulMod : PartialityMod
    {
        public DodgeOverhaulMod()
        {
            this.ModID = "Dodge Overhaul";
            this.Version = "0001";
            this.author = "Elec0";
        }

        public static DodgeBehavior testBehavior;

        public override void OnEnable()
        {
            base.OnEnable();
            DodgeBehavior.mod = this;
            
            GameObject obj = new GameObject();
            testBehavior = obj.AddComponent<DodgeBehavior>();
            testBehavior.Initialize();
        }

        public override void OnLoad()
        {
            Debug.Log("Successfully loaded");
        }
    }
}
