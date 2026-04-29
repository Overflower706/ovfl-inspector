using System;
using UnityEngine;

namespace ovfl.Inspector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class LabelTextAttribute : PropertyAttribute
    {
        public string Text { get; }

        public LabelTextAttribute(string text)
        {
            Text = text;
        }
    }
}
