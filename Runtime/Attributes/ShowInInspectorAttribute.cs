using System;

namespace ovfl.Inspector
{
    /// <summary>직렬화되지 않은 필드·프로퍼티를 Inspector에 표시합니다. (커스텀 에디터 필요)</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ShowInInspectorAttribute : Attribute { }
}
