namespace ovfl.Inspector
{
    /// <summary>HideWhenAttribute와 동일합니다. 조건이 참일 때 필드를 숨깁니다.</summary>
    public class HideIfAttribute : HideWhenAttribute
    {
        public HideIfAttribute(string conditionName) : base(conditionName) { }
    }
}
