using UnityEditor;
using UnityEngine;

namespace ovfl.Inspector.Editor
{
    [CustomPropertyDrawer(typeof(InfoBoxAttribute))]
    public class InfoBoxDecoratorDrawer : DecoratorDrawer
    {
        private const float Padding = 4f;

        private InfoBoxAttribute Attr => (InfoBoxAttribute)attribute;

        public override void OnGUI(Rect position)
        {
            var rect = new Rect(position.x, position.y + Padding, position.width, position.height - Padding);
            EditorGUI.HelpBox(rect, Attr.Message, ToMessageType(Attr.Type));
        }

        public override float GetHeight()
        {
            float textHeight = EditorStyles.helpBox.CalcHeight(
                new GUIContent(Attr.Message), EditorGUIUtility.currentViewWidth - 40f);
            return Mathf.Max(textHeight + 8f, 30f) + Padding;
        }

        private static MessageType ToMessageType(InfoBoxType type) => type switch
        {
            InfoBoxType.Warning => MessageType.Warning,
            InfoBoxType.Error   => MessageType.Error,
            _                   => MessageType.Info,
        };
    }
}
