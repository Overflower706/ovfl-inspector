using UnityEditor;
using UnityEngine;

namespace ovfl.Inspector.Editor
{
    [CustomPropertyDrawer(typeof(MinValueAttribute))]
    public class MinValuePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, property, label, true);
            if (EditorGUI.EndChangeCheck())
            {
                float min = ((MinValueAttribute)attribute).Min;
                if (property.propertyType == SerializedPropertyType.Float)
                    property.floatValue = Mathf.Max(property.floatValue, min);
                else if (property.propertyType == SerializedPropertyType.Integer)
                    property.intValue = Mathf.Max(property.intValue, (int)min);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}
