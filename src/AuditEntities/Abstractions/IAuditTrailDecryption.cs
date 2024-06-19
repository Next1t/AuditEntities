namespace AuditEntities.Abstractions;
public interface IAuditEntitiesDecryption
{
    string? Decrypt(byte[]? cipherText, bool includesHash);
}
