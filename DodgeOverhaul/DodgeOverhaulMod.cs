using Partiality.Modloader;
using UnityEngine;

namespace DodgeOverhaul
{
    public class DodgeOverhaulMod : PartialityMod
    {
        // TODO: Make minimum dodge with backpack configurable
        public DodgeOverhaulMod()
        {
            this.ModID = "Dodge Overhaul";
            this.Version = "0002";
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
            //Debug.Log("Successfully loaded");
        }
    }
}
