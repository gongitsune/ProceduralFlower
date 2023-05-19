using UnityEngine;

namespace Projects.ProceduralFlower.Scripts
{
    [ExecuteInEditMode]
    public class FlowerLeaf : MonoBehaviour
    {
        [SerializeField] private FlowerShape shape;

        private void Start()
        {
            TryGetComponent(out MeshFilter meshFilter);
            meshFilter.mesh = shape.BuildMesh();
        }
    }
}