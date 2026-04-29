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
    /// - [BoxGroup] / [FoldoutGroup] : 필드 그룹화
    /// - [Title] : 섹션 제목 (직렬화 필드·메서드·ShowInInspector 프로퍼티 지원)
    /// - [InfoBox] : 안내 박스 (직렬화 필드·메서드·ShowInInspector 프로퍼티 지원)
    /// - [Button] + [HorizontalGroup] : 버튼 및 가로 배치
    /// - [ShowInInspector] : 비직렬화 프로퍼티·필드 표시
    /// </summary>
    [CustomEditor(typeof(MonoBehaviour), true)]
    [CanEditMultipleObjects]
    public class OvflMonoBehaviourEditor : UnityEditor.Editor
    {
        private static readonly Dictionary<string, bool> FoldoutStates = new();

        private static GUIStyle boxStyle;
        private static GUIStyle boxHeaderStyle;
        private static GUIStyle foldoutHeaderStyle;
        private static GUIStyle titleLabelStyle;

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

            titleLabelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize  = 12,
                fontStyle = FontStyle.Bold
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
            var items = CollectSerializedItems(targetType);
            RenderItems(items);
        }

        private struct PropItem
        {
            public SerializedProperty Prop;
            public string             PrimaryGroup;
            public bool               IsFoldout;
            public bool               ShowBoxLabel;
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

        private void RenderItems(List<PropItem> items)
        {
            string currentGroup   = null;
            bool   currentFoldout = false;
            bool   foldoutIsOpen  = true;

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                bool groupChanged = item.PrimaryGroup != currentGroup;

                if (groupChanged && currentGroup != null)
                {
                    CloseGroup(currentFoldout, foldoutIsOpen);
                    currentGroup = null;
                }

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

                if (currentFoldout && !foldoutIsOpen) continue;

                EditorGUILayout.PropertyField(item.Prop, true);
            }

            if (currentGroup != null)
                CloseGroup(currentFoldout, foldoutIsOpen);
        }

        private void OpenBoxGroup(string groupName, bool showLabel)
        {
            EditorGUILayout.BeginVertical(boxStyle);
            if (showLabel && !string.IsNullOrEmpty(groupName))
                EditorGUILayout.LabelField(groupName, boxHeaderStyle);
        }

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

        private static void CloseGroup(bool isFoldout, bool foldoutIsOpen)
        {
            _ = isFoldout;
            _ = foldoutIsOpen;
            EditorGUILayout.EndVertical();
            GUILayout.Space(2f);
        }

        // ─── ShowInInspector 프로퍼티 / 필드 ───

        private struct ShowMember
        {
            public MemberInfo    Info;
            public string        Name;
            public Func<object>  GetValue;
            public string        FoldoutGroup;
            public int           Order;
        }

        private void DrawShowInInspectorMembers()
        {
            var target     = serializedObject.targetObject;
            var targetType = target.GetType();
            var flags      = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var props = targetType.GetProperties(flags)
                .Where(p => p.GetCustomAttribute<ShowInInspectorAttribute>() != null && p.CanRead);

            var fields = targetType.GetFields(flags)
                .Where(f => f.GetCustomAttribute<ShowInInspectorAttribute>() != null
                         && f.GetCustomAttribute<SerializeField>() == null
                         && !f.IsPublic);

            var members = new List<ShowMember>();

            foreach (var prop in props)
            {
                var captured = prop;
                members.Add(new ShowMember
                {
                    Info         = prop,
                    Name         = prop.Name,
                    GetValue     = () => SafeGet(() => captured.GetValue(target)),
                    FoldoutGroup = prop.GetCustomAttribute<FoldoutGroupAttribute>()?.GroupName,
                    Order        = prop.MetadataToken
                });
            }

            foreach (var field in fields)
            {
                var captured = field;
                members.Add(new ShowMember
                {
                    Info         = field,
                    Name         = field.Name,
                    GetValue     = () => SafeGet(() => captured.GetValue(target)),
                    FoldoutGroup = field.GetCustomAttribute<FoldoutGroupAttribute>()?.GroupName,
                    Order        = field.MetadataToken
                });
            }

            if (members.Count == 0) return;

            var ordered   = members.OrderBy(m => m.Order).ToList();
            var ungrouped = ordered.Where(m => m.FoldoutGroup == null).ToList();
            var groups    = ordered.Where(m => m.FoldoutGroup != null)
                                   .GroupBy(m => m.FoldoutGroup)
                                   .ToList();

            EditorGUILayout.Space(4f);

            foreach (var member in ungrouped)
            {
                DrawMemberDecorators(member.Info);
                EditorGUI.BeginDisabledGroup(true);
                DrawReadOnlyValue(ObjectNames.NicifyVariableName(member.Name), member.GetValue());
                EditorGUI.EndDisabledGroup();
            }

            foreach (var group in groups)
            {
                var key    = GetFoldoutKey("si_" + group.Key);
                var isOpen = FoldoutStates.TryGetValue(key, out bool v) ? v : true;
                EditorGUILayout.BeginVertical(boxStyle);
                bool newOpen = EditorGUILayout.Foldout(isOpen, group.Key, true, foldoutHeaderStyle);
                FoldoutStates[key] = newOpen;
                if (newOpen)
                {
                    foreach (var member in group)
                    {
                        DrawMemberDecorators(member.Info);
                        EditorGUI.BeginDisabledGroup(true);
                        DrawReadOnlyValue(ObjectNames.NicifyVariableName(member.Name), member.GetValue());
                        EditorGUI.EndDisabledGroup();
                    }
                }
                EditorGUILayout.EndVertical();
            }
        }

        // ─── Button 메서드 ───

        private void DrawButtons()
        {
            var targetType = serializedObject.targetObject.GetType();
            var flags      = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var methods = targetType.GetMethods(flags)
                .Where(m => m.GetCustomAttribute<ButtonAttribute>() != null && m.GetParameters().Length == 0)
                .OrderBy(m => m.MetadataToken)
                .ToArray();

            if (methods.Length == 0) return;

            EditorGUILayout.Space(4f);

            var foldoutGroups = new Dictionary<string, List<MethodInfo>>();
            var ungrouped     = new List<MethodInfo>();

            foreach (var m in methods)
            {
                var fg = m.GetCustomAttribute<FoldoutGroupAttribute>();
                if (fg != null)
                {
                    if (!foldoutGroups.TryGetValue(fg.GroupName, out var list))
                        foldoutGroups[fg.GroupName] = list = new();
                    list.Add(m);
                }
                else ungrouped.Add(m);
            }

            DrawMethodSequence(ungrouped);

            foreach (var (groupName, groupMethods) in foldoutGroups)
            {
                var key    = GetFoldoutKey("btn_" + groupName);
                var isOpen = FoldoutStates.TryGetValue(key, out bool v) ? v : true;
                EditorGUILayout.BeginVertical(boxStyle);
                bool newOpen = EditorGUILayout.Foldout(isOpen, groupName, true, foldoutHeaderStyle);
                FoldoutStates[key] = newOpen;
                if (newOpen)
                    DrawMethodSequence(groupMethods);
                EditorGUILayout.EndVertical();
            }
        }

        // 메서드 목록을 순서대로 렌더링. Title / InfoBox / HorizontalGroup 지원.
        private void DrawMethodSequence(IList<MethodInfo> methods)
        {
            int i = 0;
            while (i < methods.Count)
            {
                var method = methods[i];
                var hgAttr = method.GetCustomAttribute<HorizontalGroupAttribute>();

                DrawMemberDecorators(method);

                if (hgAttr != null)
                {
                    string groupName = hgAttr.GroupName;
                    var groupMethods = new List<MethodInfo>();
                    while (i < methods.Count)
                    {
                        var m  = methods[i];
                        var hg = m.GetCustomAttribute<HorizontalGroupAttribute>();
                        if (hg != null && hg.GroupName == groupName)
                        {
                            groupMethods.Add(m);
                            i++;
                        }
                        else break;
                    }

                    EditorGUILayout.BeginHorizontal();
                    foreach (var m in groupMethods)
                        DrawButton(m);
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    DrawButton(method);
                    i++;
                }
            }
        }

        private void DrawButton(MethodInfo method)
        {
            var attr  = method.GetCustomAttribute<ButtonAttribute>();
            var color = method.GetCustomAttribute<GUIColorAttribute>();

            string label = !string.IsNullOrEmpty(attr.Label)
                ? attr.Label
                : ObjectNames.NicifyVariableName(method.Name);

            float height = attr.Size switch
            {
                ButtonSizes.Small   => 20f,
                ButtonSizes.Large   => 32f,
                ButtonSizes.Massive => 48f,
                _                   => 24f,
            };

            if (color != null) GUI.backgroundColor *= color.Color;
            if (GUILayout.Button(label, GUILayout.Height(height)))
            {
                foreach (var t in targets)
                {
                    method.Invoke(t, null);
                    EditorUtility.SetDirty(t);
                }
            }
            if (color != null) GUI.backgroundColor = Color.white;
        }

        // ─── 데코레이터 헬퍼 ───

        // Title / InfoBox 를 GUI.enabled = true 상태에서 그린다 (BeginDisabledGroup 안에서도 올바르게 표시)
        private static void DrawMemberDecorators(MemberInfo member)
        {
            var title     = member.GetCustomAttribute<TitleAttribute>();
            var infoBoxes = member.GetCustomAttributes<InfoBoxAttribute>().ToArray();

            if (title == null && infoBoxes.Length == 0) return;

            bool wasEnabled = GUI.enabled;
            GUI.enabled = true;

            if (title != null) DrawTitle(title.Text);
            foreach (var ib in infoBoxes) DrawInfoBox(ib);

            GUI.enabled = wasEnabled;
        }

        private static void DrawTitle(string text)
        {
            GUILayout.Space(4f);
            var lineRect = EditorGUILayout.GetControlRect(false, 1f);
            EditorGUI.DrawRect(lineRect, new Color(0.5f, 0.5f, 0.5f, 0.4f));
            EditorGUILayout.LabelField(text, titleLabelStyle);
            GUILayout.Space(2f);
        }

        private static void DrawInfoBox(InfoBoxAttribute ib)
        {
            EditorGUILayout.HelpBox(ib.Message, ToMessageType(ib.Type));
        }

        private static MessageType ToMessageType(InfoBoxType type) => type switch
        {
            InfoBoxType.Warning => MessageType.Warning,
            InfoBoxType.Error   => MessageType.Error,
            _                   => MessageType.Info,
        };

        // ─── 유틸리티 ───

        private string GetFoldoutKey(string groupName)
        {
            int id = targets.Length == 1 ? serializedObject.targetObject.GetInstanceID() : 0;
            return $"{id}_{groupName}";
        }

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
                case bool b:                 EditorGUILayout.Toggle(label, b);        break;
                case int  i:                 EditorGUILayout.IntField(label, i);      break;
                case float f:                EditorGUILayout.FloatField(label, f);    break;
                case string s:               EditorGUILayout.TextField(label, s);     break;
                case Vector2 v2:             EditorGUILayout.Vector2Field(label, v2); break;
                case Vector3 v3:             EditorGUILayout.Vector3Field(label, v3); break;
                case Enum e:                 EditorGUILayout.LabelField(label, e.ToString()); break;
                case IDictionary dict:       EditorGUILayout.LabelField(label, $"{{ {dict.Count} 항목 }}"); break;
                case ICollection col:        EditorGUILayout.LabelField(label, $"[ {col.Count} 항목 ]");    break;
                case UnityEngine.Object obj: EditorGUILayout.ObjectField(label, obj, obj.GetType(), true);   break;
                default:                     EditorGUILayout.LabelField(label, value.ToString()); break;
            }
        }
    }

    [CustomEditor(typeof(ScriptableObject), true)]
    [CanEditMultipleObjects]
    public class OvflScriptableObjectEditor : OvflMonoBehaviourEditor { }
}
