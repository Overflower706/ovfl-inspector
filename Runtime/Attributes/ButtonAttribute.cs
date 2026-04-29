using System;

namespace ovfl.Inspector
{
    /// <summary>메서드에 붙이면 Inspector에서 클릭 가능한 버튼으로 표시됩니다.</summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ButtonAttribute : Attribute
    {
        public string Label { get; }
        public ButtonSizes Size { get; }

        public ButtonAttribute(string label = "", ButtonSizes size = ButtonSizes.Medium)
        {
            Label = label;
            Size = size;
        }

        public ButtonAttribute(ButtonSizes size)
        {
            Label = "";
            Size = size;
        }
    }
}
