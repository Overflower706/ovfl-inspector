using UnityEditor;
using UnityEngine;

namespace ovfl.Inspector.Editor
{
    [CustomPropertyDrawer(typeof(TitleAttribute))]
    public class TitleDecoratorDrawer : DecoratorDrawer
    {
        private TitleAttribute Attr => (TitleAttribute)attribute;

        public override void OnGUI(Rect position)
        {
            var lineRect  = new Rect(position.x, position.y + 6f, position.width, 1f);
            var labelRect = new Rect(position.x, position.y + 10f, position.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.DrawRect(lineRect, new Color(0.5f, 0.5f, 0.5f, 0.4f));
            EditorGUI.LabelField(labelRect, Attr.Text, EditorStyles.boldLabel);
        }

        public override float GetHeight() => EditorGUIUtility.singleLineHeight + 14f;
    }
}
