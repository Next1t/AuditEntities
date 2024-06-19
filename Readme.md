- [1. AuditEntities](#1-auditentities)
  - [1.1. Features](#11-features)
  - [1.2. Getting Started](#12-getting-started)
  - [1.3. Usage](#13-usage)
    - [1.3.1. Implement IAuditEntitiesConsumer Example:](#131-implement-iAuditEntitiesconsumer-example)
    - [1.3.2. Add Services Example:](#132-add-services-example)
    - [1.3.3. Create Rule Example:](#133-create-rule-example)
    - [1.3.4. Create Custom Rule Example:](#134-create-custom-rule-example)
  - [1.5. Limitations](#14-limitations)
  - [1.5. Contributing](#15-contributing)
  - [1.6. License](#16-license)


# 1. AuditEntities
AuditEntities is a tool meticulously crafted to gather vital entity data from the change tracker post DbContext SaveChanges operation.

## 1.1 Features
Audits necessary entities for logging or preserving modified property data.
Offers a convenient fluent configuration to specify tracked entities with ease.

## 1.2 Getting Started
Install the package via NuGet Package Manager or use the following command:

```bash
Install-Package AuditEntities
```
## 1.3 Usage
 - Implement `IAuditEntitiesConsumer` interface.
- Add AddAuditEntities services by providing `IAuditEntitiesConsumer` implementation.
- Create a rules derived from `EntityRule`.

### 1.3.1. Implement IAuditEntitiesConsumer Example:
 Implement the `IAuditEntitiesConsumer` interface.
To be able to retrive modified entities data after save operation.
```csharp
public class AuditEntitiesConsumer<TPermission>() : IAuditEntitiesConsumer<TPermission>
{
   public async Task ConsumeAsync(IEnumerable<AuditEntitiesDataBeforeSave<TPermission>> auditData, 
        DbContextEventData? eventData, 
        CancellationToken cancellationToken = default)
   {
        // Handle tracked entites here.
   }
}
```

### 1.3.2. Add Services Example:

```csharp
builder.Services.AddAuditEntities<PermissionType?, AuditEntitiesConsumer<PermissionType?>>(typeof(Registration).Assembly);

services.AddDbContextPool<SomeDbContext>((sp, options) =>
{
   options.UseAuditEntities<PermissionType?>(sp);
});
```

### 1.3.3 Create rule example:

Create a rule derived from `EntityRule`.

```csharp
public class UserRule : EntityRule<UserEntity, PermissionType?>
{
   public UserRule(ISomeService someService)
   {
      SetPermission(PermissionType.Users_Read);

      RuleFor(s => s.PasswordHash).Ignore();
      RuleFor(s => s.Status).ChangeName("UserStatus");
   }
}
```

### 1.3.4 Create Custom Rule Example:
You can create your own rules and modify properties as needed.
```csharp
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

public static class RuleExtensions
{
    public static IRuleBulder<T, TPermission, TProperty?> ChangeName<T, TPermission, TProperty>(this IRuleBulder<T, TPermission, TProperty> ruleBuilder, string name)
        where T : class
    {
        var rule = new ChangeNamePropertyRule<T, TProperty?>(name);
        return ruleBuilder.SetRule(rule!)!;
    }
}
```


## 1.4 Limitations

- In case of composite keys only first key will be selected as entityId.

## 1.5. Contributing

Contributions are welcome! Please submit a pull request or open an issue to propose changes or report bugs.

## 1.6 License

Sitname.AuditEntities is licensed under the MIT License.
