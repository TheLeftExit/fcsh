using System.Diagnostics.CodeAnalysis;
using System.Numerics;

public readonly struct Rational
{
    private readonly BigInteger _top;
    private readonly BigInteger _bottom;
    private readonly bool _sign;

    public Rational(BigInteger top, BigInteger bottom, bool sign)
    {
        if (bottom == 0) throw new ArgumentException(nameof(bottom));
        if (bottom < 0)
        {
            sign = !sign;
            bottom = -bottom;
        }

        if (top == 0)
        {
            (_top, _bottom, _sign) = (0, 1, true);
            return;
        }
        if (top < 0)
        {
            sign = !sign;
            top = -top;
        }

        var gdc = BigInteger.GreatestCommonDivisor(top, bottom);
        (_top, _bottom, _sign) = (top / gdc, bottom / gdc, sign);
    }

    public static implicit operator Rational(int value)
    {
        return new(value, 1, true);
    }

    public static implicit operator Rational(decimal value)
    {
        var bottom = 1;
        for (int i = 0; i < value.Scale; i++)
        {
            bottom *= 10;
        }
        return new(new(value * bottom), bottom, true);
    }

    public static Rational operator -(Rational value) => new(value._top, value._bottom, !value._sign);
    public static bool operator ==(Rational left, Rational right) => (left._top, left._bottom, left._sign) == (right._top, right._bottom, right._sign);

    public static bool operator >(Rational left, Rational right) => left._top * right._bottom * (left._sign ? 1 : -1) > right._top * left._bottom * (right._sign ? 1 : -1);
    public static Rational operator +(Rational left, Rational right) => new(left._top * right._bottom * (left._sign ? 1 : -1) + right._top * left._bottom * (right._sign ? 1 : -1), left._bottom * right._bottom, true);
    public static Rational operator *(Rational left, Rational right) => new(left._top * right._top, left._bottom * right._bottom, left._sign == right._sign);
    public static Rational operator /(Rational left, Rational right) => new(left._top * right._bottom, left._bottom * right._top, left._sign == right._sign);

    public static bool operator !=(Rational left, Rational right) => !(left == right);
    public static bool operator <(Rational left, Rational right) => right > left;
    public static bool operator >=(Rational left, Rational right) => left == right || left > right;
    public static bool operator <=(Rational left, Rational right) => left == right || left < right;
    public static Rational operator -(Rational left, Rational right) => left + (-right);

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Rational value && this == value;
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(_top);
        hash.Add(_bottom);
        hash.Add(_sign);
        return hash.ToHashCode();
    }
}