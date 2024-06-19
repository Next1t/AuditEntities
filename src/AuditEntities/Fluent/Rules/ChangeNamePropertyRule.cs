namespace AuditEntities.Fluent.Rules;
public class ChangeNamePropertyRule<TEntity, TProperty> : PropertyRule<TEntity, TProperty>
{
    private readonly string _changeName;

    public override NameValue ExecuteRule(string name, object value)
    {
        return new NameValue(_changeName, value);
    }

    public ChangeNamePropertyRule(string name)
    {
        _changeName = name;
    }
}