using System;
using UnityEngine;

namespace ovfl.Inspector
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class MinValueAttribute : PropertyAttribute
    {
        public float Min { get; }

        public MinValueAttribute(float min)
        {
            Min = min;
        }
    }
}
