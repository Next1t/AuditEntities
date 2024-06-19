namespace AuditEntities.Fluent.Rules;

public class ChangeValuePropertyRule<T, TProperty> : PropertyRule<T, TProperty?>
{
    private readonly Func<TProperty?, object?> _changeValue;

    public override NameValue ExecuteRule(string name, object value)
    {
        TProperty? prop = default;

        if (value is TProperty)
        {
            prop = (TProperty)value;
        }
        else if (value == null)
        {
            prop = default;
        }
        else
        {
            throw new InvalidCastException($"Cannot cast {value.GetType()} to {typeof(TProperty)}");
        }

        var customValue = _changeValue(prop);
        return new NameValue(name, customValue);
    }

    public ChangeValuePropertyRule(Func<TProperty?, object?> changeValue)
    {
        _changeValue = changeValue;
    }
}
