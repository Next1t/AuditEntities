using AuditEntities.Abstractions;

namespace AuditEntities.Fluent.Rules;
public class DecryptPropertyRule<TEntity, TProperty> : PropertyRule<TEntity, TProperty>
{
    private readonly bool _includeHash;
    private readonly IAuditEntitiesDecryption _AuditEntitiesDecryption;

    public override NameValue ExecuteRule(string name, object value)
    {
        try
        {
            var bytes = (byte[])value;

            var decriptedValue = _AuditEntitiesDecryption.Decrypt(bytes, _includeHash);

            return new NameValue(name, decriptedValue!);
        }
        catch (Exception)
        {
            throw new InvalidOperationException($"Only byte[] type decryption is supported, invalid type: " +
                $"{value.GetType().FullName} entity: " +
                $"{typeof(TEntity).FullName} property: {name}");
        }
    }

    public DecryptPropertyRule(IAuditEntitiesDecryption AuditEntitiesDecryption, bool includeHash)
    {
        _includeHash = includeHash;
        _AuditEntitiesDecryption = AuditEntitiesDecryption;
    }
}