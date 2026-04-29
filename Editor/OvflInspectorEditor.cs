using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ovfl.Inspector.Editor
{
    /// <summary>
    /// MonoBehaviour / ScriptableObject 공통 커스텀 에디터.
    /// - [BoxGroup] : 박스 테두리로 필드를 묶어 표시
    /// - [FoldoutGroup] : 접히는 헤더로 필드를 묶어 표시
    /// - [Button] : 메서드를 Inspector 버튼으로 표시
    /// - [ShowInInspector] : 비직렬화 프로퍼티·필드를 읽기 전용으로 표시
    /// </summary>
    [CustomEditor(typeof(MonoBehaviour), true)]
    [CanEditMultipleObjects]
    public class OvflMonoBehaviourEditor : UnityEditor.Editor
    {
        // 에디터 세션 내에서 폴드아웃 상태를 유지하는 캐시
        private static readonly Dictionary<string, bool> FoldoutStates = new();

        // ─── GUI 스타일 캐시 ───
        private static GUIStyle boxStyle;
        private static GUIStyle boxHeaderStyle;
        private static GUIStyle foldoutHeaderStyle;

        private static void EnsureStyles()
        {
            if (boxStyle != null) return;

            boxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(8, 8, 6, 6),
                margin  = new RectOffset(0, 0, 2, 4)
            };

            boxHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 11
            };

            foldoutHeaderStyle = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold,
                fontSize  = 11
            };
        }

        // ─── 메인 OnInspectorGUI ───

        public override void OnInspectorGUI()
        {
            EnsureStyles();
            serializedObject.Update();

            DrawScriptField();
            DrawGroupedSerializedProperties();

            serializedObject.ApplyModifiedProperties();

            DrawShowInInspectorMembers();
            DrawButtons();
        }

        // ─── Script 필드 ───

        private void DrawScriptField()
        {
            var scriptProp = serializedObject.FindProperty("m_Script");
            if (scriptProp == null) return;
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(scriptProp);
            EditorGUI.EndDisabledGroup();
        }

        // ─── 직렬화 필드 (BoxGroup / FoldoutGroup 지원) ───

        private void DrawGroupedSerializedProperties()
        {
            var targetType = serializedObject.targetObject.GetType();

            // 직렬화된 최상위 프로퍼티를 순서대로 수집
            var items = CollectSerializedItems(targetType);
            RenderItems(items);
        }

        // ── 수집 ──

        private struct PropItem
        {
            public SerializedProperty Prop;
            public string             PrimaryGroup; // FoldoutGroup 이름 (없으면 BoxGroup 이름)
            public bool               IsFoldout;    // true → FoldoutGroup, false → BoxGroup
            public bool               ShowBoxLabel; // BoxGroup.ShowLabel
        }

        private List<PropItem> CollectSerializedItems(Type targetType)
        {
            var result = new List<PropItem>();
            var iter   = serializedObject.GetIterator();
            bool enter = true;

            while (iter.NextVisible(enter))
            {
                enter = false;
                if (iter.name == "m_Script") continue;

                var field       = FindFieldInfo(targetType, iter.name);
                var boxAttr     = field?.GetCustomAttribute<BoxGroupAttribute>();
                var foldoutAttr = field?.GetCustomAttribute<FoldoutGroupAttribute>();

                string primary;
                bool   isFoldout;
                bool   showLabel;

                if (foldoutAttr != null)
                {
                    primary   = foldoutAttr.GroupName;
                    isFoldout = true;
                    showLabel = false;
                }
                else if (boxAttr != null)
                {
                    primary   = boxAttr.GroupName;
                    isFoldout = false;
                    showLabel = boxAttr.ShowLabel;
                }
                else
                {
                    primary   = null;
                    isFoldout = false;
                    showLabel = false;
                }

                result.Add(new PropItem
                {
                    Prop         = iter.Copy(),
                    PrimaryGroup = primary,
                    IsFoldout    = isFoldout,
                    ShowBoxLabel = showLabel
                });
            }

            return result;
        }

        // ── 렌더링 ──

        private void RenderItems(List<PropItem> items)
        {
            string currentGroup    = null;
            bool   currentFoldout  = false;
            bool   foldoutIsOpen   = true;

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                bool groupChanged = item.PrimaryGroup != currentGroup;

                // ─ 그룹이 달라지면 이전 그룹 닫기 ─
                if (groupChanged && currentGroup != null)
                {
                    CloseGroup(currentFoldout, foldoutIsOpen);
                    currentGroup = null;
                }

                // ─ 새 그룹 열기 ─
                if (groupChanged && item.PrimaryGroup != null)
                {
                    currentGroup   = item.PrimaryGroup;
                    currentFoldout = item.IsFoldout;

                    if (item.IsFoldout)
                    {
                        foldoutIsOpen = OpenFoldoutGroup(item.PrimaryGroup);
                    }
                    else
                    {
                        OpenBoxGroup(item.PrimaryGroup, item.ShowBoxLabel);
                        foldoutIsOpen = true;
                    }
                }

                // ─ 프로퍼티 그리기 (폴드아웃이 닫혀 있으면 생략) ─
                if (currentFoldout && !foldoutIsOpen) continue;

                EditorGUILayout.PropertyField(item.Prop, true);
            }

            // ─ 마지막 그룹 닫기 ─
            if (currentGroup != null)
                CloseGroup(currentFoldout, foldoutIsOpen);
        }

        // ─ BoxGroup 열기 ─
        private void OpenBoxGroup(string groupName, bool showLabel)
        {
            EditorGUILayout.BeginVertical(boxStyle);
            if (showLabel && !string.IsNullOrEmpty(groupName))
                EditorGUILayout.LabelField(groupName, boxHeaderStyle);
        }

        // ─ FoldoutGroup 열기; 펼쳐졌는지 반환 ─
        private bool OpenFoldoutGroup(string groupName)
        {
            var key = GetFoldoutKey(groupName);
            if (!FoldoutStates.TryGetValue(key, out bool isOpen))
                isOpen = true;

            EditorGUILayout.BeginVertical(boxStyle);
            bool newOpen = EditorGUILayout.Foldout(isOpen, groupName, true, foldoutHeaderStyle);
            FoldoutStates[key] = newOpen;
            return newOpen;
        }

        // ─ 그룹 닫기 ─
        private static void CloseGroup(bool isFoldout, bool foldoutIsOpen)
        {
            _ = isFoldout; // foldout/box 모두 BeginVertical을 사용하므로 동일하게 닫음
            _ = foldoutIsOpen;
            EditorGUILayout.EndVertical();
            GUILayout.Space(2f);
        }

        private string GetFoldoutKey(string groupName)
        {
            int id = targets.Length == 1 ? serializedObject.targetObject.GetInstanceID() : 0;
            return $"{id}_{groupName}";
        }

        // ─── ShowInInspector 프로퍼티 ───

        private void DrawShowInInspectorMembers()
        {
            var target     = serializedObject.targetObject;
            var targetType = target.GetType();
            var flags      = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            // ShowInInspector가 붙은 C# 프로퍼티
            var props = targetType.GetProperties(flags)
                .Where(p => p.GetCustomAttribute<ShowInInspectorAttribute>() != null && p.CanRead)
                .ToArray();

            // ShowInInspector가 붙은 비직렬화 필드 (SerializeField 없는 private)
            var fields = targetType.GetFields(flags)
                .Where(f => f.GetCustomAttribute<ShowInInspectorAttribute>() != null
                         && f.GetCustomAttribute<SerializeField>() == null
                         && !f.IsPublic)
                .ToArray();

            if (props.Length == 0 && fields.Length == 0) return;

            // FoldoutGroup으로 그룹화
            var groups = new Dictionary<string, List<(string name, Func<object> getValue)>>();
            var ungrouped = new List<(string name, Func<object> getValue)>();

            foreach (var prop in props)
            {
                var fg = prop.GetCustomAttribute<FoldoutGroupAttribute>();
                Add(fg?.GroupName, prop.Name, () => SafeGet(() => prop.GetValue(target)));
            }
            foreach (var field in fields)
            {
                var fg = field.GetCustomAttribute<FoldoutGroupAttribute>();
                Add(fg?.GroupName, field.Name, () => SafeGet(() => field.GetValue(target)));
            }

            void Add(string group, string name, Func<object> getter)
            {
                if (group != null)
                {
                    if (!groups.TryGetValue(group, out var list)) groups[group] = list = new();
                    list.Add((name, getter));
                }
                else ungrouped.Add((name, getter));
            }

            EditorGUILayout.Space(4f);
            EditorGUI.BeginDisabledGroup(true);

            // 그룹 없는 것 먼저
            foreach (var (name, get) in ungrouped)
                DrawReadOnlyValue(ObjectNames.NicifyVariableName(name), get());

            // FoldoutGroup 별로 렌더링
            foreach (var (groupName, members) in groups)
            {
                var key    = GetFoldoutKey("si_" + groupName);
                var isOpen = FoldoutStates.TryGetValue(key, out bool v) ? v : true;
                EditorGUILayout.BeginVertical(boxStyle);
                bool newOpen = EditorGUILayout.Foldout(isOpen, groupName, true, foldoutHeaderStyle);
                FoldoutStates[key] = newOpen;
                if (newOpen)
                    foreach (var (name, get) in members)
                        DrawReadOnlyValue(ObjectNames.NicifyVariableName(name), get());
                EditorGUILayout.EndVertical();
            }

            EditorGUI.EndDisabledGroup();
        }

        // ─── Button 메서드 ───

        private void DrawButtons()
        {
            var targetType = serializedObject.targetObject.GetType();
            var flags      = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var methods = targetType.GetMethods(flags)
                .Where(m => m.GetCustomAttribute<ButtonAttribute>() != null && m.GetParameters().Length == 0)
                .ToArray();

            if (methods.Length == 0) return;

            // FoldoutGroup 별로 그룹화
            var groups    = new Dictionary<string, List<MethodInfo>>();
            var ungrouped = new List<MethodInfo>();

            foreach (var m in methods)
            {
                var fg = m.GetCustomAttribute<FoldoutGroupAttribute>();
                if (fg != null)
                {
                    if (!groups.TryGetValue(fg.GroupName, out var list)) groups[fg.GroupName] = list = new();
                    list.Add(m);
                }
                else ungrouped.Add(m);
            }

            EditorGUILayout.Space(4f);

            foreach (var method in ungrouped)
                DrawButton(method);

            foreach (var (groupName, groupMethods) in groups)
            {
                var key    = GetFoldoutKey("btn_" + groupName);
                var isOpen = FoldoutStates.TryGetValue(key, out bool v) ? v : true;
                EditorGUILayout.BeginVertical(boxStyle);
                bool newOpen = EditorGUILayout.Foldout(isOpen, groupName, true, foldoutHeaderStyle);
                FoldoutStates[key] = newOpen;
                if (newOpen)
                    foreach (var method in groupMethods)
                        DrawButton(method);
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawButton(MethodInfo method)
        {
            var attr  = method.GetCustomAttribute<ButtonAttribute>();
            var color = method.GetCustomAttribute<GUIColorAttribute>();
            string label = !string.IsNullOrEmpty(attr.Label)
                ? attr.Label
                : ObjectNames.NicifyVariableName(method.Name);

            if (color != null) GUI.backgroundColor *= color.Color;
            if (GUILayout.Button(label))
            {
                foreach (var t in targets)
                {
                    method.Invoke(t, null);
                    EditorUtility.SetDirty(t);
                }
            }
            if (color != null) GUI.backgroundColor = Color.white;
        }

        // ─── 유틸리티 ───

        private static FieldInfo FindFieldInfo(Type type, string name)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            while (type != null && type != typeof(object))
            {
                var f = type.GetField(name, flags);
                if (f != null) return f;
                type = type.BaseType;
            }
            return null;
        }

        private static object SafeGet(Func<object> fn)
        {
            try { return fn(); } catch { return null; }
        }

        private static void DrawReadOnlyValue(string label, object value)
        {
            if (value == null) { EditorGUILayout.LabelField(label, "null"); return; }
            switch (value)
            {
                case bool b:                EditorGUILayout.Toggle(label, b);       break;
                case int  i:                EditorGUILayout.IntField(label, i);     break;
                case float f:               EditorGUILayout.FloatField(label, f);   break;
                case string s:              EditorGUILayout.TextField(label, s);    break;
                case Vector2 v2:            EditorGUILayout.Vector2Field(label, v2);break;
                case Vector3 v3:            EditorGUILayout.Vector3Field(label, v3);break;
                case Enum e:                EditorGUILayout.LabelField(label, e.ToString()); break;
                case IDictionary dict:      EditorGUILayout.LabelField(label, $"{{ {dict.Count} 항목 }}"); break;
                case ICollection col:       EditorGUILayout.LabelField(label, $"[ {col.Count} 항목 ]");    break;
                case UnityEngine.Object obj:EditorGUILayout.ObjectField(label, obj, obj.GetType(), true);   break;
                default:                    EditorGUILayout.LabelField(label, value.ToString()); break;
            }
        }
    }

    [CustomEditor(typeof(ScriptableObject), true)]
    [CanEditMultipleObjects]
    public class OvflScriptableObjectEditor : OvflMonoBehaviourEditor { }
}
