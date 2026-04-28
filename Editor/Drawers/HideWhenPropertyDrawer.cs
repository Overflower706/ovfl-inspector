using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ovfl.Inspector.Editor
{
    [CustomPropertyDrawer(typeof(HideWhenAttribute))]
    public class HideWhenPropertyDrawer : PropertyDrawer
    {
        // 조건을 확인하여 '숨길지 말지'를 결정하는 메서드
        private bool ShouldHide(SerializedProperty property)
        {
            HideWhenAttribute hideWhenAttribute = (HideWhenAttribute)attribute;
            string conditionName = hideWhenAttribute.ConditionName;

            object targetObject = ReflectionUtility.GetTargetObjectWithProperty(property);

            if (targetObject == null)
            {
                Debug.LogWarning($"[ovfl.Inspector] 부모 객체를 찾을 수 없습니다. (대상: {property.name})");
                return false; // 부모 객체를 못 찾았을 때는 숨기지 않습니다.
            }

            Type targetType = targetObject.GetType();

            // 1. Field 확인
            FieldInfo field = targetType.GetField(conditionName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null && field.FieldType == typeof(bool)) return (bool)field.GetValue(targetObject);

            // 2. Property 확인
            PropertyInfo prop = targetType.GetProperty(conditionName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null && prop.PropertyType == typeof(bool)) return (bool)prop.GetValue(targetObject);

            // 3. Method 확인
            MethodInfo method = targetType.GetMethod(conditionName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null && method.ReturnType == typeof(bool) && method.GetParameters().Length == 0)
                return (bool)method.Invoke(targetObject, null);

            // 조건을 못 찾았을 때는 기본적으로 보여줍니다 (숨기지 않음 = false).
            Debug.LogWarning($"[ovfl.Inspector] '{conditionName}' 조건을 찾을 수 없거나 반환 타입이 bool이 아닙니다. (대상: {property.name})");
            return false;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // ShouldHide가 false일 때(숨기지 않아야 할 때)만 그립니다. (!)
            if (!ShouldHide(property))
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // 숨기지 않을 때는 원래 프로퍼티의 높이를 반환합니다.
            if (!ShouldHide(property))
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }

            // 💡 꿀팁: 숨길 때 0f를 반환하면 프로퍼티 간의 기본 간격(2px)이 남아 여백이 생깁니다.
            // 완전히 밀착시켜서 숨기려면 standardVerticalSpacing을 빼주어야 합니다.
            return -EditorGUIUtility.standardVerticalSpacing;
        }
    }
}
