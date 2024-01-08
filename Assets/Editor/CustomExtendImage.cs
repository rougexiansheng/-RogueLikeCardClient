using UnityEditor;

namespace EditorTool.CustomToolBar
{
    [CustomEditor(typeof(ExtendImage))]
    public class CustomExtendImageEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            ExtendImage image = (ExtendImage)target;

            EditorGUILayout.LabelField("Extend Settings", EditorStyles.boldLabel);

            image.extendTop = EditorGUILayout.Toggle("Extend Top", image.extendTop);
            image.extendBottom = EditorGUILayout.Toggle("Extend Bottom", image.extendBottom);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Image Settings", EditorStyles.boldLabel);

            DrawDefaultInspector();
        }
    }
}