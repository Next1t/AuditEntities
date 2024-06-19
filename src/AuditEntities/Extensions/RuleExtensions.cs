using AuditEntities.Fluent.Abstractions;
using AuditEntities.Fluent.Rules;

namespace AuditEntities.Extensions;
public static class RuleExtensions
{
    public static IRuleBuilder<T, TPermission, TProperty?> Ignore<T, TPermission, TProperty>(this IRuleBuilder<T, TPermission, TProperty> ruleBuilder)
        where T : class
    {
        var rule = new IgnorePropertyRule<T, TProperty?>();
        return ruleBuilder.SetRule(rule!)!;
    }

    public static IRuleBuilder<T, TPermission, TProperty?> ChangeName<T, TPermission, TProperty>(this IRuleBuilder<T, TPermission, TProperty> ruleBuilder, string name)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(name);
        var rule = new ChangeNamePropertyRule<T, TProperty?>(name);
        return ruleBuilder.SetRule(rule!)!;
    }

    public static IRuleBuilder<T, TPermission, TProperty?> ChangeValue<T, TPermission, TProperty>(this IRuleBuilder<T, TPermission, TProperty?> ruleBuilder, Func<TProperty?, object> func)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(func);
        return ruleBuilder.SetRule(new ChangeValuePropertyRule<T, TProperty>(property => func(property)));
    }
}