using System;
using UnityEngine;

namespace ovfl.Inspector
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class LabelWidthAttribute : PropertyAttribute
    {
        public float Width { get; }

        public LabelWidthAttribute(float width)
        {
            Width = width;
        }
    }
}
