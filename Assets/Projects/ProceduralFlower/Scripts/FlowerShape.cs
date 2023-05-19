using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Projects.ProceduralFlower.Scripts
{
    [CreateAssetMenu(menuName = "ProceduralFlower/Shape")]
    public class FlowerShape : ScriptableObject
    {
        [SerializeField] private float size = 1;
        [SerializeField] private int numberOfVerticesOnOneSide = 20;
        [SerializeField] private int resolution = 2;
        [SerializeField] private float noiseDepth = 0.25f;
        [SerializeField] private Vector2 noiseScale = Vector2.zero;
        public List<FlowerControlPoint> controls;

        public Mesh BuildMesh()
        {
            return BuildMesh(size, controls, numberOfVerticesOnOneSide, resolution, noiseDepth, noiseScale);
        }

        public static List<Vector3> Mirror(List<FlowerControlPoint> controls, float height, float width)
        {
            var bottom = new Vector3(0f, 0f, 0f);
            var top = new Vector3(0f, height, 0f);

            var points = new List<Vector3> { bottom };
            points.AddRange(controls.Select(p => new Vector3(p.width * width, p.height * height, 0f)));
            points.Add(top);

            var reverse = controls.ToList();
            reverse.Reverse();
            points.AddRange(reverse.Select(p => new Vector3(-p.width * width, p.height * height, 0f)));

            return points;
        }

        public static Mesh BuildMesh(float size, List<FlowerControlPoint> controls, int numberOfVerticesOnOneSide = 20,
            int resolution = 2, float noiseDepth = 0.25f, Vector2 noiseScale = default, Vector2 noiseOffset = default)
        {
            var mesh = new Mesh();

            resolution = Mathf.Max(1, resolution);

            if (controls.Count < 1) throw new UnityException("control points size is not enough");

            var controlSize = controls.Count;

            var counts = GetCounts(controls, numberOfVerticesOnOneSide);
            var countOnEdge = counts.Aggregate((acc, next) => acc + next);

            var bottom = new Vector3(0f, 0f, 0f);
            var top = new Vector3(0f, size, 0f);

            var points = new List<Vector3> { bottom };
            points.AddRange(controls.Select(p => new Vector3(p.width * size, p.height * size, 0f)));
            points.Add(top);

            var reverse = controls.ToList();
            reverse.Reverse();
            points.AddRange(reverse.Select(p => new Vector3(-p.width * size, p.height * size, 0f)));

            var vertices = new List<Vector3>();
            var uv = new List<Vector2>();

            // left side
            for (var i = 0; i < resolution; i++)
            {
                var r = (float)(resolution - i) / resolution;
                for (var c = 0; c <= controlSize; c++)
                {
                    var edge = FlowerCatmullRomSpline.GetCatmullRomPositions(
                        counts[c],
                        GetEdge(GetLoopPoint(points, c - 1), r),
                        GetEdge(GetLoopPoint(points, c), r),
                        GetEdge(GetLoopPoint(points, c + 1), r),
                        GetEdge(GetLoopPoint(points, c + 2), r)
                    );
                    vertices.AddRange(edge);
                    uv.AddRange(edge.Select(v => new Vector2(v.x / size, v.y / size)));
                }
            }

            // midrib 
            for (var c = 0; c <= controlSize; c++)
            {
                var edge = FlowerCatmullRomSpline.GetCatmullRomPositions(
                    counts[c],
                    GetEdge(GetLoopPoint(points, c - 1), 0f),
                    GetEdge(GetLoopPoint(points, c), 0f),
                    GetEdge(GetLoopPoint(points, c + 1), 0f),
                    GetEdge(GetLoopPoint(points, c + 2), 0f)
                );
                vertices.AddRange(edge);
                uv.AddRange(edge.Select(v => new Vector2(0f, v.y / size)));
            }

            // right side
            for (var i = 0; i < resolution; i++)
            {
                var r = -(float)(i + 1) / resolution;
                for (var c = 0; c <= controlSize; c++)
                {
                    var edge = FlowerCatmullRomSpline.GetCatmullRomPositions(
                        counts[c],
                        GetEdge(GetLoopPoint(points, c - 1), r),
                        GetEdge(GetLoopPoint(points, c), r),
                        GetEdge(GetLoopPoint(points, c + 1), r),
                        GetEdge(GetLoopPoint(points, c + 2), r)
                    );
                    vertices.AddRange(edge);
                    uv.AddRange(edge.Select(v => new Vector2(v.x / size, v.y / size)));
                }
            }

            var triangles = new List<int>();

            // left side faces
            for (var i = 0; i < resolution; i++)
            {
                var offset = i * countOnEdge;
                for (var j = 0; j < countOnEdge - 1; j++)
                {
                    var a = j + offset;
                    var b = a + 1;
                    var c = a + countOnEdge;
                    var d = b + countOnEdge;
                    triangles.Add(a);
                    triangles.Add(b);
                    triangles.Add(c);
                    triangles.Add(b);
                    triangles.Add(d);
                    triangles.Add(c);
                }
            }

            // right side faces
            for (var i = 0; i < resolution; i++)
            {
                var offset = (i + resolution) * countOnEdge;
                for (var j = 0; j < countOnEdge - 1; j++)
                {
                    var a = j + offset;
                    var b = a + 1;
                    var c = a + countOnEdge;
                    var d = b + countOnEdge;
                    triangles.Add(a);
                    triangles.Add(d);
                    triangles.Add(c);
                    triangles.Add(b);
                    triangles.Add(d);
                    triangles.Add(a);
                }
            }

            var nOffset = noiseOffset + new Vector2(size, size);
            mesh.vertices = vertices
                .Select(v => new Vector3(v.x, v.y, v.y * noiseDepth * Depth(v, nOffset, noiseScale))).ToArray();
            mesh.uv = uv.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            return mesh;
        }

        private static int[] GetCounts(IReadOnlyCollection<FlowerControlPoint> controls, int count)
        {
            var controlSize = controls.Count;

            var points = controls.Select(c => new Vector2(c.width, c.height)).ToList();

            var acc = 0f;
            var lengthArray = new float[controlSize + 1];
            for (int i = 0, n = points.Count; i <= n; i++)
            {
                float l;
                if (i == 0)
                    l = points[0].magnitude;
                else if (i < n)
                    l = (points[i] - points[i - 1]).magnitude;
                else
                    l = (new Vector2(0f, 1f) - points[i - 1]).magnitude;
                acc += l;
                lengthArray[i] = l;
            }

            return lengthArray.Select(l => Mathf.FloorToInt(l / acc * count)).ToArray();
        }

        private static Vector2 GetEdge(Vector2 p, float ratio)
        {
            return new Vector2(p.x * ratio, p.y);
        }

        public static Vector2 GetLoopPoint(IReadOnlyList<Vector3> points, int index)
        {
            while (true)
            {
                if (index < 0)
                {
                    index = points.Count + index;
                    continue;
                }

                if (index < points.Count) return points[index];
                index -= points.Count;
            }
        }

        private static float Depth(Vector3 v, Vector2 noiseOffset, Vector2 noiseScale)
        {
            return Mathf.PerlinNoise((v.x + noiseOffset.x) * noiseScale.x, (v.y + noiseOffset.y) * noiseScale.y) - 0.5f;
        }
    }
}