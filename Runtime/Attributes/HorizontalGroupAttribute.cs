using System;
using UnityEngine;

namespace ovfl.Inspector
{
    /// <summary>필드를 수평으로 배치합니다. (완전한 수평 레이아웃은 미지원 — 스텁)</summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
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
