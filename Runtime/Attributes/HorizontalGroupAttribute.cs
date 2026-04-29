using System;
using UnityEngine;

namespace ovfl.Inspector
{
    /// <summary>필드 또는 메서드(버튼)를 수평으로 배치합니다.</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HorizontalGroupAttribute : PropertyAttribute
    {
        public string GroupName { get; }
        public float Width { get; set; }

        public HorizontalGroupAttribute(string groupName = "", float width = 0f)
        {
            GroupName = groupName;
            Width = width;
        }
    }
}
