using System;
using UnityEngine;

namespace Projects.ProceduralFlower.Scripts
{
    [Serializable]
    public class FlowerControlPoint
    {
        public float width;
        public float height;

        public FlowerControlPoint(float width, float height)
        {
            this.width = Mathf.Clamp01(width);
            this.height = Mathf.Clamp01(height);
        }
    }
}