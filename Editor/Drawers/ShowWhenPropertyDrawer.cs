// ovfl.Inspector/Editor/Drawers/ShowWhenPropertyDrawer.cs
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ovfl.Inspector.Editor
{
    [CustomPropertyDrawer(typeof(ShowWhenAttribute), true)]
    public class ShowWhenPropertyDrawer : PropertyDrawer
    {
        private bool ShouldShow(SerializedProperty property)
        {
            ShowWhenAttribute showWhenAttribute = (ShowWhenAttribute)attribute;
            string conditionName = showWhenAttribute.ConditionFieldName;

            // 핵심 수정 포인트: 최상위 오브젝트가 아니라 '현재 속한 오브젝트 인스턴스'를 가져옴
            object targetObject = ReflectionUtility.GetTargetObjectWithProperty(property);

            if (targetObject == null)
            {
                Debug.LogWarning($"[ovfl.Inspector] 부모 객체를 찾을 수 없습니다. (대상: {property.name})");
                return true;
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

            Debug.LogWarning($"[ovfl.Inspector] '{conditionName}' 조건을 찾을 수 없거나 반환 타입이 bool이 아닙니다. (대상: {property.name})");
            return true;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (ShouldShow(property))
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (ShouldShow(property))
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }
            return 0f; // HideWhen에서는 -EditorGUIUtility.standardVerticalSpacing
        }
    }
}
