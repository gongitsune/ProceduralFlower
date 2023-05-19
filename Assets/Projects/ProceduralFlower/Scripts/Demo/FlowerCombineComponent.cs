using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace Projects.ProceduralFlower.Scripts.Demo
{
    [RequireComponent(typeof(MeshFilter))]
    public class FlowerCombineComponent : MonoBehaviour
    {
        [SerializeField] private string outputPath;
        [SerializeField] private FlowerPartType partType = FlowerPartType.None;
        [SerializeField] private FlowerData flower;

        private void Awake()
        {
            var mesh = FlowerCombine.Combine(flower, partType);
            SaveMesh(mesh); // Editor Only
            TryGetComponent(out MeshFilter meshFilter);
            meshFilter.sharedMesh = mesh;
        }

        [Conditional("UNITY_EDITOR")]
        private void SaveMesh(Object mesh)
        {
            AssetDatabase.CreateAsset(mesh, outputPath);
        }
    }
}