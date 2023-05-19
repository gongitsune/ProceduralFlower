using System;
using System.Collections.Generic;
using UnityEngine;

namespace Projects.ProceduralFlower.Scripts
{
    [Serializable]
    public enum FlowerPartType
    {
        None,
        Petal,
        Stover
    }

    public class FlowerPart : MonoBehaviour
    {
        public const float Epsilon = 0.1f;
        private static readonly int PropertyColor = Shader.PropertyToID("_Color2");
        private static readonly int PropertyBend = Shader.PropertyToID("_Bend");
        private static readonly int PropertyT = Shader.PropertyToID("_T");
        public List<FlowerSegment> children = new();
        [SerializeField] private FlowerPartType type = FlowerPartType.None;

        private bool _animating;

        private MaterialPropertyBlock _block;
        private float _multiplySpeed = 1f;
        private MeshRenderer _renderer;
        private float _speed = 1f;
        private float _ticker;

        public FlowerPartType Type => type;

        private MeshRenderer Rnd
        {
            get
            {
                if (_renderer == null) _renderer = GetComponent<MeshRenderer>();
                return _renderer;
            }
        }

        private MaterialPropertyBlock Block
        {
            get
            {
                if (_block != null) return _block;
                _block = new MaterialPropertyBlock();
                Rnd.GetPropertyBlock(_block);
                return _block;
            }
        }

        private void Update()
        {
            if (!_animating) return;

            _ticker += Time.deltaTime * _multiplySpeed * _speed;
            Fade(_ticker);
            children.ForEach(anim => anim.Animate(_speed, _ticker));
            if (_ticker > 1f + Epsilon) _animating = false;
        }

        public void SetType(FlowerPartType tp)
        {
            type = tp;
        }

        public void Colorize(Color color)
        {
            if (type == FlowerPartType.None) return;

            Block.SetColor(PropertyColor, color);
            Rnd.SetPropertyBlock(Block);
        }

        public void Bend(float bend)
        {
            if (type == FlowerPartType.None) return;

            Block.SetFloat(PropertyBend, bend);
            Rnd.SetPropertyBlock(Block);
        }

        public void Fade(float t)
        {
            if (type == FlowerPartType.None) return;

            Block.SetFloat(PropertyT, t);
            Rnd.SetPropertyBlock(Block);
        }

        public void SetSpeed(float m)
        {
            _multiplySpeed = m;
        }

        public void Add(FlowerPart part, float ratio)
        {
            children.Add(new FlowerSegment(part, ratio));
        }

        public void Animate(float s = 1f)
        {
            _speed = s;
            _animating = true;
            _ticker = 0f;
        }

        [Serializable]
        public class FlowerSegment
        {
            public readonly FlowerPart Part;
            public readonly float Ratio;

            private bool _animating = false;

            public FlowerSegment(FlowerPart p, float r)
            {
                Part = p;
                Ratio = r;
            }

            public void Animate(float speed, float r)
            {
                if (!_animating && r <= Ratio) Part.Animate(speed);
            }
        }
    }
}