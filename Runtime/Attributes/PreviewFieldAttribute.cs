using System;
using UnityEngine;

namespace ovfl.Inspector
{
    /// <summary>오브젝트 필드에 프리뷰 이미지를 표시합니다. (Unity 기본 오브젝트 필드로 대체 — 스텁)</summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class PreviewFieldAttribute : PropertyAttribute
    {
        public float Height { get; }

        public PreviewFieldAttribute(float height = 60f)
        {
            Height = height;
        }

        public PreviewFieldAttribute(float height, ObjectFieldAlignment alignment)
        {
            Height = height;
        }
    }

    public enum ObjectFieldAlignment { Left, Center, Right }
}
