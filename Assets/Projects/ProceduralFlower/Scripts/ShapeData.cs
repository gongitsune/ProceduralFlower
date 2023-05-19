using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Projects.ProceduralFlower.Scripts
{
    [Serializable]
    internal class ShapeData
    {
        [SerializeField] private FlowerShape shape;
        public Material material;
        [HideInInspector] public Mesh mesh;
        public ShadowCastingMode shadowCastingMode = ShadowCastingMode.On;
        public bool receiveShadows = true;

        public void Init()
        {
            mesh = shape.BuildMesh();
            if (material == null) Debug.LogWarning("ShapeData material is null");
        }
    }
}