using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Projects.ProceduralFlower.Scripts.Editor
{
    [CustomEditor(typeof(FlowerShape))]
    public class FlowerShapeInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI () {
            base.OnInspectorGUI();

            if(GUILayout.Button("Open Editor")) {
                FlowerShapeWindow.Open();
            }
        }
    }
}