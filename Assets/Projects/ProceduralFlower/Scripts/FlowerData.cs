using System;
using System.Collections.Generic;
using Projects.ProceduralFlower.Scripts.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace Projects.ProceduralFlower.Scripts
{
    [CreateAssetMenu(menuName = "ProceduralFlower/Flower Data")]
    public class FlowerData : ScriptableObject
    {
        [SerializeField] private ShapeData budData;
        [SerializeField] private ShapeData petalData;
        [SerializeField] private ShapeData leafData;

        [HideInInspector] public float height = 2f;
        [HideInInspector] public int leafCount = 6;
        [HideInInspector] public Vector2 leafScaleRange = new(0.2f, 0.825f);
        [HideInInspector] public Vector2 leafSegmentRange = new(0.2f, 0.92f);
        [SerializeField] private StemData stemData;

        public GameObject Build(bool visible = true)
        {
            _rand = new RandomUtil(seed);

            budData.Init();
            petalData.Init();
            leafData.Init();
            stemData.Init();

            var root = CreateStem("Root", stemData.Stem, stemData.shadowCastingMode, stemData.receiveShadows, r => 1f,
                height, stemData.bend, visible);
            var stemPart = root.GetComponent<FlowerPart>();

            var segments = stemData.Stem.Segments;
            var segmentOffset = leafSegmentRange.x * segments.Count;
            var len = (leafSegmentRange.y - leafSegmentRange.x) * segments.Count;
            var size = 1f;

            for (var i = 0; i < leafCount; i++)
            {
                var r = (float)(i + 1) / (leafCount + 1);
                var index = Mathf.Min(Mathf.FloorToInt(len * r + segmentOffset), segments.Count - 2);
                var from = segments[index];
                var to = segments[index + 1];
                var dir = (to.Position - from.Position).normalized;
                var leaf = CreateLeaf(segments[index], dir, i % 4 * 90f + _rand.SampleRange(-20f, 20f), visible);
                leaf.transform.SetParent(root.transform);

                // lower leaf becomes bigger than upper one.
                size = _rand.SampleRange(size, 1f - r * 0.5f);
                leaf.transform.localScale *= Mathf.Lerp(leafScaleRange.x, leafScaleRange.y, size);

                var leafPart = leaf.GetComponent<FlowerPart>();
                stemPart.Add(leafPart, r);
            }

            var flower = CreateFlower(visible);
            flower.transform.SetParent(root.transform);
            flower.transform.localPosition = stemData.Stem.Tip.Position;
            flower.transform.localRotation *=
                stemData.Stem.Tip.Rotation * Quaternion.FromToRotation(Vector3.back, Vector3.up);

            stemPart.Add(flower.GetComponent<FlowerPart>(), 1f);

            return root;
        }

        private GameObject CreateFlower(bool visible)
        {
            var floret = new Florets();

            var bud = CreateShape("Bud", FlowerPartType.Petal, budData, visible);
            var petal = CreateShape("Petal", FlowerPartType.Petal, petalData, visible);

            var flower = new GameObject("Flower");
            var root = flower.AddComponent<FlowerPart>();
            root.SetType(FlowerPartType.None);

            var inv = 1f / n;
            for (var i = 0; i < n; i++)
            {
                var r = i * inv;

                var p = Florets.Get(i + 1, c);

                GameObject go;

                if (i < m)
                {
                    go = Instantiate(bud);
                    go.transform.localScale = Vector3.one * (1f + Mathf.Max(min, p.magnitude)) * scale * 0.75f;
                }
                else
                {
                    go = Instantiate(petal);
                    go.transform.localScale = Vector3.one * (1f + Mathf.Max(min, p.magnitude)) * scale;
                }

                var part = go.GetComponent<FlowerPart>();
                part.Colorize(new Color(_rand.SampleRange(0.5f, 1f), _rand.SampleRange(0.5f, 1f),
                    _rand.SampleRange(0.5f, 1f)));
                part.Bend(1f - r);
                part.Fade(visible ? 1f + FlowerPart.Epsilon : 0f);

                go.transform.SetParent(flower.transform, false);

                go.transform.localPosition = p + Vector3.down * r * offset;
                go.transform.localRotation = Quaternion.LookRotation(Vector3.up, p.normalized) *
                                             Quaternion.AngleAxis((1f - r * angleScale) * angle, Vector3.right);

                root.Add(go.GetComponent<FlowerPart>(), r);
            }

            if (Application.isPlaying)
            {
                Destroy(bud);
                Destroy(petal);
            }
            else
            {
                DestroyImmediate(bud);
                DestroyImmediate(petal);
            }

            return flower;
        }

        private static GameObject CreateBase(string name, FlowerPartType type, Mesh mesh, Material material,
            ShadowCastingMode shadowCastingMode, bool receiveShadows, bool visible)
        {
            var go = new GameObject(name);
            go.AddComponent<MeshFilter>().mesh = mesh;

            var rnd = go.AddComponent<MeshRenderer>();
            rnd.sharedMaterial = material;
            rnd.shadowCastingMode = shadowCastingMode;
            rnd.receiveShadows = receiveShadows;

            var part = go.AddComponent<FlowerPart>();
            part.SetType(type);
            part.Fade(visible ? 1f + FlowerPart.Epsilon : 0f);

            return go;
        }

        private static GameObject CreateShape(string name, FlowerPartType type, ShapeData data, bool visible)
        {
            return CreateBase(name, type, data.mesh, data.material, data.shadowCastingMode, data.receiveShadows,
                visible);
        }

        private GameObject CreateStem(string name, FlowerStem stem, ShadowCastingMode shadowCastingMode,
            bool receiveShadows, Func<float, float> f, float height, float bend, bool visible)
        {
            var controls = GetControls(4, height, bend);
            var mesh = stem.Build(controls, f);
            return CreateBase(name, FlowerPartType.Stover, mesh, stemData.material, stemData.shadowCastingMode,
                stemData.receiveShadows, visible);
        }

        private GameObject CreateLeaf(Point segment, Vector3 dir, float angle, bool visible)
        {
            var stem = new FlowerStem(10, 2, 0.01f);
            var root = CreateStem("Stem", stem, leafData.shadowCastingMode, leafData.receiveShadows,
                r => Mathf.Max(1f - r, 0.2f), 0.05f, 0.0f, visible);
            root.transform.localPosition = segment.Position;
            root.transform.localRotation *= Quaternion.FromToRotation(Vector3.forward, dir) *
                                            Quaternion.AngleAxis(angle, Vector3.forward);

            var leaf = CreateShape("Leaf", FlowerPartType.Stover, leafData, visible);
            leaf.transform.SetParent(root.transform, false);
            leaf.transform.localPosition = stem.Tip.Position;
            leaf.transform.localRotation *= Quaternion.AngleAxis(_rand.SampleRange(0f, 30f), Vector3.up);

            var part = root.GetComponent<FlowerPart>();
            part.SetSpeed(5f);
            part.Add(leaf.GetComponent<FlowerPart>(), 1f);

            return root;
        }

        private List<Vector3> GetControls(int count, float height, float radius)
        {
            var controls = new List<Vector3>();
            count = Mathf.Max(4, count);
            for (var i = 0; i < count; i++)
            {
                var r = (float)i / (count - 1);
                var circle = _rand.SampleUnitCircle() * radius;
                controls.Add(new Vector3(circle.x, r * height, circle.y));
            }

            return controls;
        }

        #region Flower Settings

        // [SerializeField, Range(137.4f, 137.6f)] float alpha = 137.5f;
        [HideInInspector] public float c = 0.01f;
        [HideInInspector] public int n = 70;
        [HideInInspector] public int m = 8;
        [HideInInspector] public float scale = 0.328f;
        [HideInInspector] public float min = 0.1f;
        [HideInInspector] public float angle = 87f;
        [HideInInspector] public float angleScale = 0.92f;
        [HideInInspector] public float offset;

        #endregion

        #region Random

        [SerializeField] private int seed;
        private RandomUtil _rand;

        #endregion
    }

    internal class Florets
    {
        private const float Angle = 137.5f * Mathf.Deg2Rad;

        public static Vector3 Get(int n, float c, float alpha = 137.5f)
        {
            // var phi = n * ANGLE;
            var phi = n * alpha * Mathf.Deg2Rad;
            var r = c * Mathf.Sqrt(n);
            return new Vector3(Mathf.Cos(phi) * r, 0f, Mathf.Sin(phi) * r);
        }
    }
}