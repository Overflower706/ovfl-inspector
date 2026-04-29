using System;
using UnityEditor;
using UnityEngine;

namespace ovfl.Inspector.Editor
{
    [CustomPropertyDrawer(typeof(EnumToggleButtonsAttribute))]
    public class EnumToggleButtonsPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Enum)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            float labelWidth = EditorGUIUtility.labelWidth;
            var labelRect = new Rect(position.x, position.y, labelWidth, position.height);
            var buttonsRect = new Rect(position.x + labelWidth, position.y, position.width - labelWidth, position.height);

            EditorGUI.LabelField(labelRect, label);

            string[] names = property.enumDisplayNames;
            int count = names.Length;
            float btnWidth = buttonsRect.width / count;

            for (int i = 0; i < count; i++)
            {
                var btnRect = new Rect(buttonsRect.x + btnWidth * i, buttonsRect.y, btnWidth, buttonsRect.height);
                bool isSelected = property.enumValueIndex == i;
                bool clicked = GUI.Toggle(btnRect, isSelected, names[i], EditorStyles.miniButtonMid);
                if (clicked && !isSelected)
                    property.enumValueIndex = i;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
