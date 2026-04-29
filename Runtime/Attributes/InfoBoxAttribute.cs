using System;
using UnityEngine;

namespace ovfl.Inspector
{
    public enum InfoBoxType { Info, Warning, Error }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
    public class InfoBoxAttribute : PropertyAttribute
    {
        public string Message { get; }
        public InfoBoxType Type { get; }

        public InfoBoxAttribute(string message, InfoBoxType type = InfoBoxType.Info)
        {
            Message = message;
            Type = type;
        }
    }
}
