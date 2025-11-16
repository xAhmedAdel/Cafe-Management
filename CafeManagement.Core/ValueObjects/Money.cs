namespace CafeManagement.Core.ValueObjects;

public readonly record struct Money(decimal Amount)
{
    public static Money Zero => new(0m);
    public static Money FromDecimal(decimal amount) => new(amount);

    public Money Add(Money other) => new(Amount + other.Amount);
    public Money Subtract(Money other) => new(Amount - other.Amount);
    public Money Multiply(decimal multiplier) => new(Amount * multiplier);
    public Money Divide(decimal divisor) => new(Amount / divisor);

    public bool IsZero => Amount == 0m;
    public bool IsNegative => Amount < 0m;
    public bool IsPositive => Amount > 0m;

    public override string ToString() => $"${Amount:F2}";
}