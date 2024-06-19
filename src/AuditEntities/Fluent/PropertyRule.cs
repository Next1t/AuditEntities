using AuditEntities.Fluent.Abstractions;
using System.Linq.Expressions;

namespace AuditEntities.Fluent;
public class PropertyRule<TEntity, TProperty> : IPropertyRule<TEntity, TProperty>
{
    private readonly List<IPropertyRule<TEntity, TProperty>> _propertyRules = new();

    private readonly string _propertyName = "";
    public string PropertyName => _propertyName;

    public IReadOnlyList<IPropertyRule<TEntity, TProperty>> ProperyRules => _propertyRules.AsReadOnly();

    public PropertyRule() { }

    public void Add(IPropertyRule<TEntity, TProperty> rule)
    {
        _propertyRules.Add(rule);
    }

    public virtual NameValue ExecuteRule(string name, object value)
    {
        NameValue? result = new NameValue(name, value);

        foreach (IPropertyRule<TEntity, TProperty> item in _propertyRules)
        {
            result = item.ExecuteRule(result.name, result.value);
            if (result is null || result.Name is null)
            {
                return null!;
            }
        }

        return result;
    }

    public static IPropertyRule<TEntity, TProperty> Create(Expression<Func<TEntity, TProperty>> expression)
    {
        return new PropertyRule<TEntity, TProperty>(expression);
    }

    public PropertyRule(Expression<Func<TEntity, TProperty>> expression)
    {
        MemberExpression body = expression.Body as MemberExpression
            ?? throw new ArgumentException("Invalid property");

        _propertyName = body.Member.Name
            ?? throw new ArgumentException("Invalid property name");
    }
}