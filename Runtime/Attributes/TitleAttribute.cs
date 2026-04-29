using System;
using UnityEngine;

namespace ovfl.Inspector
{
    /// <summary>Inspector에 섹션 제목을 표시합니다.</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class TitleAttribute : PropertyAttribute
    {
        public string Text { get; }
        public TitleAttribute(string text) { Text = text; }
    }
}
