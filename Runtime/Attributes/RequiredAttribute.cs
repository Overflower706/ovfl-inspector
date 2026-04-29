using System;
using UnityEngine;

namespace ovfl.Inspector
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class RequiredAttribute : PropertyAttribute { }
}
