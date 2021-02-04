﻿public partial class PayoutType
{
    public struct Type
    {
        private int _Value;

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
    public static readonly Type Hint = 2;
    public static readonly Type Other = 3;
    public static readonly Type NoAds = 4;
}