using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Projects.ProceduralFlower.Scripts
{
    public static class FlowerCombine
    {
        public static Mesh Combine(FlowerData flower, FlowerPartType type = FlowerPartType.None)
        {
            var root = flower.Build();
            var filters = new List<MeshFilter>();
            Traverse(root.GetComponent<FlowerPart>(), filters);

            if (type != FlowerPartType.None)
                filters = filters.FindAll(filter => filter.GetComponent<FlowerPart>().Type == type).ToList();

            var combine = new CombineInstance[filters.Count];
            for (int i = 0, n = filters.Count; i < n; i++)
            {
                combine[i].mesh = filters[i].sharedMesh;
                combine[i].transform = filters[i].transform.localToWorldMatrix;
            }

            Object.Destroy(root);

            var mesh = new Mesh();
            mesh.CombineMeshes(combine);
            return mesh;
        }

        private static void Traverse(FlowerPart root, ICollection<MeshFilter> filters)
        {
            var filter = root.GetComponent<MeshFilter>();
            if (filter != null) filters.Add(filter);

            root.children.ForEach(child => { Traverse(child.Part, filters); });
        }
    }
}