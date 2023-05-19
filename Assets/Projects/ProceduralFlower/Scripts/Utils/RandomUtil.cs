using UnityEngine;
using Random = System.Random;

namespace Projects.ProceduralFlower.Scripts.Utils
{
    public class RandomUtil
    {
        private readonly Random _rnd;

        public RandomUtil(int seed)
        {
            _rnd = new Random(seed);
        }

        public float Sample01()
        {
            return (float)_rnd.NextDouble();
        }

        public int SampleRange(int a, int b)
        {
            var t = Sample01();
            return Mathf.FloorToInt(Mathf.Lerp(a, b, t));
        }

        public float SampleRange(float a, float b)
        {
            var t = Sample01();
            return Mathf.Lerp(a, b, t);
        }

        public Vector2 SampleUnitCircle()
        {
            var x = (Sample01() - 0.5f) * 2f;
            var y = (Sample01() - 0.5f) * 2f;
            return new Vector2(x, y);
        }

        public Vector3 SampleUnitSphere()
        {
            var x = (Sample01() - 0.5f) * 2f;
            var y = (Sample01() - 0.5f) * 2f;
            var z = (Sample01() - 0.5f) * 2f;
            return new Vector3(x, y, z);
        }
    }
}