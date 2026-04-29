namespace ovfl.Inspector
{
    /// <summary>ShowWhenAttribute와 동일합니다. 조건이 참일 때 필드를 표시합니다.</summary>
    public class ShowIfAttribute : ShowWhenAttribute
    {
        public ShowIfAttribute(string conditionFieldName) : base(conditionFieldName) { }
    }
}
