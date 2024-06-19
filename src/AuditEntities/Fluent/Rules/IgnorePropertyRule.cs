namespace AuditEntities.Fluent.Rules;
public class IgnorePropertyRule<TEntity, TProperty> : PropertyRule<TEntity, TProperty>
{
    public override NameValue ExecuteRule(string name, object value)
    {
        return null!;
    }
}