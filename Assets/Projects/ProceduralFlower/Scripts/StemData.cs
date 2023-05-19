using UnityEngine;
using UnityEngine.Rendering;

namespace Projects.ProceduralFlower.Scripts
{
    [System.Serializable]
    internal class StemData {
        [HideInInspector] public FlowerStem Stem;

        public Material material = null;
        [SerializeField] private int wResolution = 10;
        [SerializeField] private int hResolution = 8;
        [SerializeField] private float radius = 0.012f;
        public float bend = 0.05f;
        public ShadowCastingMode shadowCastingMode = ShadowCastingMode.On;
        public bool receiveShadows = true;

        public void Init () {
            Stem = new FlowerStem(wResolution, hResolution, radius);
            if(material == null) {
                Debug.LogWarning("StemData material is null");
            }
        }
    }
}