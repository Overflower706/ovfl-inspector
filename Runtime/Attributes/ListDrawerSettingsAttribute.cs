using System;
using UnityEngine;

namespace ovfl.Inspector
{
    /// <summary>리스트 드로어 설정. (Unity ReorderableList로 기본 지원 — 추가 설정은 스텁)</summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ListDrawerSettingsAttribute : PropertyAttribute
    {
        public bool ShowIndexLabels { get; set; }
        public bool DraggableItems { get; set; } = true;
        public bool Expanded { get; set; }
        public string ListElementLabelName { get; set; }
    }
}
