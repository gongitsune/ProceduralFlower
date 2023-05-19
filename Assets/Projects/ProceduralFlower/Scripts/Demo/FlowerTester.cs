using UnityEngine;

namespace Projects.ProceduralFlower.Scripts.Demo
{
    public class FlowerTester : MonoBehaviour
    {
        public FlowerData flower;

        private GameObject _child;

        private void Start()
        {
            Build();
        }

        public void Build()
        {
            Clear();
            _child = flower.Build();
            _child.transform.SetParent(transform, false);
        }

        public void Clear()
        {
            if (_child == null) return;
            if (Application.isPlaying)
                Destroy(_child);
            else
                DestroyImmediate(_child);
        }
    }
}