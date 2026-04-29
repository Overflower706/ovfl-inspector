using System;
using UnityEngine;

namespace ovfl.Inspector
{
    /// <summary>리스트를 테이블 형식으로 표시합니다. (일반 리스트로 대체 — 스텁)</summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class TableListAttribute : PropertyAttribute
    {
        public bool AlwaysExpanded { get; set; }
        public bool ShowIndexLabels { get; set; }
    }
}
