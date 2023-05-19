using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Projects.ProceduralFlower.Scripts
{
    public class Point
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public Point(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }
    }

    public class FlowerStem
    {
        public readonly int HResolution = 4;
        public readonly float Radius = 0.05f;
        public readonly int WResolution = 10;
        private List<Point> _segments = new();

        public FlowerStem()
        {
        }

        public FlowerStem(int hResolution, int wResolution, float radius)
        {
            HResolution = hResolution;
            Radius = radius;
            WResolution = wResolution;
        }

        public Point Tip { get; private set; }
        public IReadOnlyList<Point> Segments => _segments;

        public Mesh Build(List<Vector3> controls)
        {
            return Build(controls, WResolution, HResolution, Radius, _ => 1f);
        }

        public Mesh Build(List<Vector3> controls, Func<float, float> f)
        {
            return Build(controls, WResolution, HResolution, Radius, f);
        }

        private Mesh Build(List<Vector3> controls, int wResolution, int hResolution, float radius, Func<float, float> f)
        {
            controls = controls.ToList();
            if (controls.Count < 4) throw new UnityException("control size is not enough");

            Vector3 first = controls[0], second = controls[1];
            Vector3 blast = controls[^2], last = controls[^1];

            controls.Insert(0, first + (first - second).normalized * 0.25f);
            controls.Add(last + (last - blast).normalized * 0.25f);

            var cores = new List<Vector3>();
            for (int i = 1, n = controls.Count - 2; i < n; i++)
            {
                var tmp = FlowerCatmullRomSpline.GetCatmullRomPositions(hResolution, controls[i - 1], controls[i],
                    controls[i + 1], controls[i + 2]);
                if (i != 1) tmp.RemoveAt(0);
                cores.AddRange(tmp);
            }

            var vertices = new List<Vector3>();
            var uv = new List<Vector2>();
            var triangles = new List<int>();

            var mesh = new Mesh();

            var circle = new List<Vector3>();
            for (var i = 0; i < wResolution; i++)
            {
                var r = (float)i / wResolution * (Mathf.PI * 2f);
                var cos = Mathf.Cos(r) * radius;
                var sin = Mathf.Sin(r) * radius;
                circle.Add(new Vector3(cos, sin, 0f));
            }

            vertices.Add(cores.First());
            var right = Vector3.right;
            var inverse = false;

            _segments = new List<Point>();

            for (int i = 0, n = cores.Count; i < n; i++)
            {
                var core = cores[i];
                var v = (float)i / (n - 1);

                if (i == 0)
                {
                    // first segment
                    for (var j = 0; j < wResolution; j++)
                    {
                        triangles.Add(0);
                        triangles.Add((j + 1) % wResolution + 1);
                        triangles.Add(j + 1);
                    }

                    var next = cores[i + 1];
                    var dir = (next - core).normalized;
                    right = (Quaternion.LookRotation(dir) * Vector3.right).normalized;

                    uv.Add(new Vector2(0.5f, 0f));
                }

                var offset = i * wResolution + 1;

                if (i < n - 1)
                {
                    var next = cores[i + 1];
                    var dir = (next - core).normalized;
                    var cr = (Quaternion.LookRotation(dir) * Vector3.right).normalized;
                    if (Vector3.Dot(right, cr) < 0f) inverse = !inverse;

                    var rotation = AddCircle(vertices, core, circle, f(v), dir, inverse);
                    _segments.Add(new Point(core, rotation));

                    right = cr;

                    for (var j = 0; j < wResolution; j++)
                    {
                        var a = offset + j;
                        var b = offset + (j + 1) % wResolution;
                        var c = a + wResolution;
                        var d = b + wResolution;
                        triangles.Add(a);
                        triangles.Add(b);
                        triangles.Add(c);
                        triangles.Add(c);
                        triangles.Add(b);
                        triangles.Add(d);

                        var u = (float)j / (wResolution - 1);
                        uv.Add(new Vector2(u, v));
                    }
                }
                else
                {
                    // last segment
                    var prev = cores[i - 1];
                    var dir = (core - prev).normalized;
                    var cr = (Quaternion.LookRotation(dir) * Vector3.right).normalized;
                    if (Vector3.Dot(right, cr) < 0f) inverse = !inverse;

                    var rotation = AddCircle(vertices, core, circle, f(v), dir, inverse);
                    Tip = new Point(core, rotation);

                    vertices.Add(cores.Last());
                    uv.Add(new Vector2(0.5f, 1f));
                    var m = vertices.Count - 1;

                    for (var j = 0; j < wResolution; j++)
                    {
                        triangles.Add(m);
                        triangles.Add(offset + j);
                        triangles.Add(offset + (j + 1) % wResolution);

                        var u = (float)j / (wResolution - 1);
                        uv.Add(new Vector2(u, v));
                    }
                }
            }

            mesh.vertices = vertices.ToArray();
            mesh.uv = uv.ToArray();
            mesh.triangles = triangles.ToArray();

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            return mesh;
        }

        private static Quaternion AddCircle(ICollection<Vector3> vertices, Vector3 core, List<Vector3> circle,
            float radius,
            Vector3 dir, bool inverse)
        {
            Quaternion q;
            if (inverse)
            {
                q = Quaternion.AngleAxis(180f, dir) * Quaternion.LookRotation(dir);
                circle.ForEach(c => { vertices.Add(core + q * (c * radius)); });
            }
            else
            {
                q = Quaternion.LookRotation(dir);
                circle.ForEach(c => { vertices.Add(core + q * (c * radius)); });
            }

            return q;
        }
    }
}