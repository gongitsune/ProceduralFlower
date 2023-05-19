using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Projects.ProceduralFlower.Scripts.Editor
{
    public class FlowerShapeWindow : EditorWindow
    {
        private const float Unit = 16f;

        private const string PackagePath = "Assets/Projects/ProceduralFlower/";

        private static Vector2 _size = new(512f, 512f);

        private Texture _grid;
        private Texture _knob;

        private Mode _mode = Mode.None;

        private List<Vector2> _points;
        private int _selected = -1;
        private FlowerShape _shape;

        private void Update()
        {
            Repaint();
        }

        private void OnGUI()
        {
            CheckInit();

            _size.x = _size.y = Mathf.Min(Screen.width, Screen.height);
            GUI.DrawTexture(new Rect(0f, 0f, _size.x, _size.y), _grid);
            DrawHeader();
            CatchEvent();

            for (int i = 0, n = _points.Count; i < n; i++)
            {
                GUI.color = i == _selected ? new Color(0.6f, 0.75f, 1f) : Color.white;
                var p = _points[i];
                GUI.DrawTexture(
                    new Rect(Vector2.Scale(p, _size) - new Vector2(Unit, Unit) * 0.5f, new Vector2(Unit, Unit)), _knob);
            }

            GUI.color = Color.white;

            // GUI.Box(new Rect(new Vector2((size.x - unit) * 0.5f, 0f), new Vector2(unit, unit)), "top");
            // GUI.Box(new Rect(new Vector2((size.x - unit) * 0.5f, size.y - unit), new Vector2(unit, unit)), "bottom");

            if (_points.Count < 2) return;
            var mirror = FlowerShape.Mirror(GetControls(_points), _size.x, _size.y);
            Handles.color = Color.black;
            DrawShape(mirror, _points.Count);
        }

        [MenuItem("ProceduralFlower/Window")]
        public static void Open()
        {
            var window = GetWindow<FlowerShapeWindow>(typeof(SceneView));
            var icon = AssetDatabase.LoadAssetAtPath<Texture>(PackagePath + "Textures/PFShapeWindow.png");
            window.titleContent = new GUIContent("PF", icon);
            window.Focus();
        }

        private void CheckInit()
        {
            var obj = Selection.activeObject;
            if (obj && obj as FlowerShape)
            {
                var current = obj as FlowerShape;
                if (current != _shape) Load(current);
                _shape = current;
            }

            if (_grid == null) _grid = AssetDatabase.LoadAssetAtPath<Texture>(PackagePath + "Textures/Grid.jpg");

            if (_knob == null) _knob = AssetDatabase.LoadAssetAtPath<Texture>(PackagePath + "Textures/Knob.png");

            _points ??= new List<Vector2>();
        }

        private static Vector2 Translate(Vector2 p)
        {
            return new Vector2(p.x + _size.x * 0.5f, p.y);
        }

        private void Load(FlowerShape shape)
        {
            var controls = shape.controls;
            _points = controls.Select(c => Convert(c)).ToList();
        }

        private void Apply()
        {
            _shape.controls = GetControls(_points);
        }

        private void DrawHeader()
        {
            var style = new GUIStyle
            {
                fontSize = 15
            };
            if (_shape)
                GUI.Label(new Rect(5f, 5f, 80f, 25f), _shape.name + ".asset", style);
            else
                GUI.Label(new Rect(5f, 5f, 80f, 25f), "Select Shape asset", style);

            if (_shape && GUI.Button(new Rect(5f, 30f, 80f, 25f), "Apply")) Apply();

            if (GUI.Button(new Rect(5f, 60f, 80f, 25f), "Clear All")) _points.Clear();
        }

        private static void DrawShape(IReadOnlyList<Vector3> mirror, int count, int resolution = 5)
        {
            // left side
            var points = new List<Vector3>();
            for (var i = 0; i <= count; i++)
            {
                var edge = FlowerCatmullRomSpline.GetCatmullRomPositions(
                    resolution,
                    FlowerShape.GetLoopPoint(mirror, i - 1),
                    FlowerShape.GetLoopPoint(mirror, i),
                    FlowerShape.GetLoopPoint(mirror, i + 1),
                    FlowerShape.GetLoopPoint(mirror, i + 2)
                );
                if (i != 0) edge.RemoveAt(0);
                points.AddRange(edge);
            }

            for (int i = 0, n = points.Count - 1; i < n; i++)
            {
                var from = points[i];
                var to = points[i + 1];
                Handles.DrawLine(Translate(from), Translate(to));
            }

            for (int i = 0, n = points.Count - 1; i < n; i++)
            {
                var from = points[i];
                var to = points[i + 1];
                Handles.DrawLine(Translate(new Vector2(-from.x, from.y)), Translate(new Vector2(-to.x, to.y)));
            }
        }

        private static Vector2 Convert(FlowerControlPoint c)
        {
            return new Vector2(0.5f - c.width, c.height);
        }

        private static FlowerControlPoint Convert(Vector2 p)
        {
            return new FlowerControlPoint(0.5f - p.x, p.y);
        }

        private static List<FlowerControlPoint> GetControls(IEnumerable<Vector2> points)
        {
            return points.Select(Convert).ToList();
        }

        private int Pick(Vector2 pos)
        {
            var distance = Unit / _size.x;
            return _points.FindIndex(p => Vector2.Distance(p, pos) <= distance);
        }

        private static bool Contains(Vector2 pos)
        {
            return pos.x is > 0.05f and < 0.5f && pos.y is > 0.05f and < 0.95f;
        }

        private void Sort()
        {
            _points.Sort((p0, p1) => p0.y.CompareTo(p1.y));
        }

        private void CatchEvent()
        {
            var mp = Event.current.mousePosition;
            var pos = new Vector2(mp.x / _size.x, mp.y / _size.y);

            switch (_mode)
            {
                case Mode.None:
                    switch (Event.current.type)
                    {
                        case EventType.MouseDown:
                            _selected = Pick(pos);
                            if (_selected >= 0) _mode = Mode.Select;
                            break;
                        case EventType.MouseUp:
                            if (Contains(pos))
                            {
                                _points.Add(pos);
                                Sort();
                            }

                            break;
                        case EventType.KeyUp:
                            if (_selected >= 0 && Input.GetKeyUp(KeyCode.Delete)) _points.RemoveAt(_selected);
                            break;
                    }

                    break;

                case Mode.Select:
                    switch (Event.current.type)
                    {
                        case EventType.MouseDrag:
                            if (_selected >= 0 && Contains(pos)) _points[_selected] = pos;
                            break;
                        case EventType.MouseUp:
                            Sort();
                            _mode = Mode.None;
                            break;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private enum Mode
        {
            None,
            Select
        }
    }
}