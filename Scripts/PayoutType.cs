using Unity.Collections;

[System.Obsolete("PayoutType is deprecated. Switch to PayoutTypeEnum")]
public partial class PayoutType
{
    [System.Obsolete("PayoutType is deprecated. Switch to PayoutTypeEnum")]
    [System.Serializable]
    public struct Type
    {
        [ReadOnly][UnityEngine.SerializeField] private int _Value;

        public static implicit operator Type(int value)
        {
            return new Type { _Value = value };
        }

        public static implicit operator int(Type value)
        {
            return value._Value;
        }

        public override string ToString()
        {
            return _Value.ToString();
        }
    }
    public static readonly Type Currency = 1;
    public static readonly Type Item = 2;
    public static readonly Type Other = 3;
    public static readonly Type NoAds = 4;
}

public enum PayoutTypeEnum
{
    Currency = 1,
    Item = 2,
    Other = 3,
    NoAds = 4
}