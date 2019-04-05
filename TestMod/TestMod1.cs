using Partiality.Modloader;
using UnityEngine;

namespace OutwardTestMod1
{
    public class TestMod1 : PartialityMod
    {
        public TestMod1()
        {
            this.ModID = "Elec0's First test mod";
            this.Version = "0001";
            this.author = "Elec0";
        }

        public static TestBehavior testBehavior;

        public override void OnEnable()
        {
            base.OnEnable();
            TestBehavior.mod = this;
            
            GameObject obj = new GameObject();
            testBehavior = obj.AddComponent<TestBehavior>();
            testBehavior.Initialize();
        }

        public override void OnLoad()
        {
            Debug.Log("Successfully loaded");
        }
    }
}
