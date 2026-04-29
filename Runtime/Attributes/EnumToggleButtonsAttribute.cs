using System;
using UnityEngine;

namespace ovfl.Inspector
{
    /// <summary>열거형 필드를 토글 버튼 그룹으로 표시합니다.</summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class EnumToggleButtonsAttribute : PropertyAttribute { }
}
