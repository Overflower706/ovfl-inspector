using System;
using UnityEngine;

namespace ovfl.Inspector
{
    /// <summary>필드를 폴더블 그룹으로 묶어 표시합니다. (시각적 그룹화 — 완전한 폴드아웃은 미지원)</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class FoldoutGroupAttribute : PropertyAttribute
    {
        public string GroupName { get; }
        public bool Expanded { get; set; }

        public FoldoutGroupAttribute(string groupName, bool expanded = true)
        {
            GroupName = groupName;
            Expanded = expanded;
        }
    }
}
