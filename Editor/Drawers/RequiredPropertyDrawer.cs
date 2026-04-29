using UnityEditor;
using UnityEngine;

namespace ovfl.Inspector.Editor
{
    [CustomPropertyDrawer(typeof(RequiredAttribute))]
    public class RequiredPropertyDrawer : PropertyDrawer
    {
        private const float WarningHeight = 30f;
        private const float Spacing = 2f;

        private bool IsEmpty(SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.ObjectReference
                && property.objectReferenceValue == null;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (IsEmpty(property))
            {
                var warningRect = new Rect(position.x, position.y, position.width, WarningHeight);
                EditorGUI.HelpBox(warningRect, $"'{label.text}' 은(는) 필수 항목입니다.", MessageType.Error);
                var fieldRect = new Rect(position.x, position.y + WarningHeight + Spacing, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(fieldRect, property, label);
            }
            else
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (IsEmpty(property))
                return WarningHeight + Spacing + EditorGUIUtility.singleLineHeight;
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}
