using System;
using UnityEngine;

namespace ovfl.Inspector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ReadOnlyAttribute : PropertyAttribute { }
}
