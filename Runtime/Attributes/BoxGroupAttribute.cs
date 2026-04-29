using System;
using UnityEngine;

namespace ovfl.Inspector
{
    /// <summary>필드를 박스 그룹으로 묶어 표시합니다. (시각적 그룹화 — 완전한 박스 렌더링은 미지원)</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class BoxGroupAttribute : PropertyAttribute
    {
        public string GroupName { get; }
        public bool ShowLabel { get; set; }

        public BoxGroupAttribute(string groupName = "", bool showLabel = false)
        {
            GroupName = groupName;
            ShowLabel = showLabel;
        }
    }
}
