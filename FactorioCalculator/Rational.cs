using System.Diagnostics.CodeAnalysis;
using System.Numerics;

public readonly struct Rational
{
    private readonly bool _initialized; // default(Rational) == (Rational)0
    public readonly BigInteger Top { get => _initialized ? field : 0; init; }
    public readonly BigInteger Bottom { get => _initialized ? field : 1; init; }
    public readonly bool Sign { get => _initialized ? field : true; init; }

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
            (Top, Bottom, Sign) = (0, 1, true);
            return;
        }
        if (top < 0)
        {
            sign = !sign;
            top = -top;
        }

        var gdc = BigInteger.GreatestCommonDivisor(top, bottom);
        (Top, Bottom, Sign) = (top / gdc, bottom / gdc, sign);
        _initialized = true;
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

    public static Rational operator -(Rational value) => new(value.Top, value.Bottom, !value.Sign);
    public static bool operator ==(Rational left, Rational right) => (left.Top, left.Bottom, left.Sign) == (right.Top, right.Bottom, right.Sign);

    public static bool operator >(Rational left, Rational right) => left.Top * right.Bottom * (left.Sign ? 1 : -1) > right.Top * left.Bottom * (right.Sign ? 1 : -1);
    public static Rational operator +(Rational left, Rational right) => new(left.Top * right.Bottom * (left.Sign ? 1 : -1) + right.Top * left.Bottom * (right.Sign ? 1 : -1), left.Bottom * right.Bottom, true);
    public static Rational operator *(Rational left, Rational right) => new(left.Top * right.Top, left.Bottom * right.Bottom, left.Sign == right.Sign);
    public static Rational operator /(Rational left, Rational right) => new(left.Top * right.Bottom, left.Bottom * right.Top, left.Sign == right.Sign);

    public static bool operator !=(Rational left, Rational right) => !(left == right);
    public static bool operator <(Rational left, Rational right) => right > left;
    public static bool operator >=(Rational left, Rational right) => left == right || left > right;
    public static bool operator <=(Rational left, Rational right) => left == right || left < right;
    public static Rational operator -(Rational left, Rational right) => left + (-right);

    public static (Rational Quotinent, Rational Remainder) DivRem(Rational left, Rational right)
    {
        var fractionalResult = left / right;
        var quotinentInteger = fractionalResult.Top / fractionalResult.Bottom;
        var quotinent = new Rational(quotinentInteger, 1, fractionalResult.Sign);
        var remainder = new Rational(fractionalResult.Top - quotinentInteger * fractionalResult.Bottom, fractionalResult.Bottom, fractionalResult.Sign);
        return (quotinent, remainder);
    }

    public static Rational Mod(Rational value) => new(value.Top, value.Bottom, true);

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Rational value && this == value;
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Top);
        hash.Add(Bottom);
        hash.Add(Sign);
        return hash.ToHashCode();
    }
    public override string ToString()
    {
        return ((decimal)Top / (decimal)Bottom * (Sign ? 1 : -1)).ToString();
    }
}