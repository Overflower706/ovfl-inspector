using UnityEditor;
using UnityEngine;

namespace ovfl.Inspector.Editor
{
    [CustomPropertyDrawer(typeof(GUIColorAttribute))]
    public class GUIColorDecoratorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = (GUIColorAttribute)attribute;
            var prev = GUI.color;
            GUI.color *= attr.Color;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.color = prev;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}
