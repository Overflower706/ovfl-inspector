using System;
using UnityEngine;

namespace ovfl.Inspector
{
    /// <summary>필드를 수직 그룹으로 묶습니다. (완전한 수직 레이아웃은 미지원 — 스텁)</summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class VerticalGroupAttribute : PropertyAttribute
    {
        public string GroupName { get; }

        public VerticalGroupAttribute(string groupName = "")
        {
            GroupName = groupName;
        }
    }
}
