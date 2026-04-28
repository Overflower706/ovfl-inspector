using System;
using UnityEngine;

namespace ovfl.Inspector
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class HideWhenAttribute : PropertyAttribute
    {
        public string ConditionName { get; private set; }

        public HideWhenAttribute(string conditionName)
        {
            ConditionName = conditionName;
        }
    }
}
