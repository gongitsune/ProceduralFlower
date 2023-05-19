using System;
using UnityEngine;

namespace Projects.ProceduralFlower.Scripts.Demo
{
    public class FlowerGrowing : MonoBehaviour
    {
        [SerializeField] private FlowerData flower;

        private void Start()
        {
            var root = flower.Build(false);
            root.transform.SetParent(transform, false);
            root.TryGetComponent(out FlowerPart flowerPart);
            flowerPart.Animate();
        }
    }
}