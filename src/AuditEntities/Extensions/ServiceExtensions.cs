namespace AuditEntities.Extensions;

using AuditEntities.Abstractions;
using AuditEntities.Fluent;
using AuditEntities.Fluent.Abstractions;
using AuditEntities.Interceptors;
using AuditEntities.Options;
using AuditEntities.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;

public static class ServiceExtensions
{
    /// <summary>
    /// Add AddAuditEntities which includes AuditEntities services
    /// </summary>
    /// <param name="TPermission">Type taht will be set in rule configuration for each entity rule</param>
    /// <param name="TConsumer">The implementation of IAuditEntitiesConsumer interface</param>
    /// <param name="services">The collection of services</param>
    /// <param name="assembly">The assembly to scan</param>
    /// <param name="lifetime">The lifetime of the AuditEntities. The default is scoped (per-request in web application)</param>
    /// <param name="filter">Optional filter that allows certain types to be skipped from registration.</param>
    /// <param name="includeInternalTypes">Include internal AuditEntities. The default is false.</param>
    /// <returns>AssemblyScanner</returns>
    public static IServiceCollection AddAuditEntities<TPermission, TConsumer>(
        this IServiceCollection services,
        Assembly assembly,
        Action<AuditEntitiesOptions>? options = null,
        ServiceLifetime lifetime = ServiceLifetime.Scoped,
        Func<AssemblyScanner.AssemblyScanResult, bool> filter = null!,
        bool includeInternalTypes = false)
        where TConsumer : class, IAuditEntitiesConsumer<TPermission>
    {
        services.AddAuditEntities<TPermission, TConsumer>([assembly], options, lifetime, filter, includeInternalTypes);

        return services;
    }

    /// <summary>
    /// Add AddAuditEntities which includes AuditEntities services
    /// </summary>
    /// <param name="TPermission">Type taht will be set in rule configuration for each entity rule</param>
    /// <param name="TConsumer">The implementation of IAuditEntitiesConsumer interface</param>
    /// <param name="services">The collection of services</param>
    /// <param name="assemblies">The assemblies to scan</param>
    /// <param name="lifetime">The lifetime of the AuditEntities. The default is scoped (per-request in web application)</param>
    /// <param name="filter">Optional filter that allows certain types to be skipped from registration.</param>
    /// <param name="includeInternalTypes">Include internal AuditEntities. The default is false.</param>
    /// <returns>AssemblyScanner</returns>
    public static IServiceCollection AddAuditEntities<TPermission, TConsumer>(
        this IServiceCollection services,
        IEnumerable<Assembly> assemblies,
        Action<AuditEntitiesOptions>? options = null,
        ServiceLifetime lifetime = ServiceLifetime.Scoped,
        Func<AssemblyScanner.AssemblyScanResult, bool> filter = null!,
        bool includeInternalTypes = false)
        where TConsumer : class, IAuditEntitiesConsumer<TPermission>
    {
        services.AddHttpContextAccessor();
        services.AddAuditEntitiesFromAssemblies<TPermission>(assemblies, lifetime, filter, includeInternalTypes);
        services.AddScoped<IAuditEntitiesService<TPermission>, AuditEntitiesService<TPermission>>();
        services.AddScoped(typeof(IAuditEntitiesConsumer<TPermission>), typeof(TConsumer));

        services.AddSingleton<AuditEntitiesSaveInterceptor<TPermission>>();
        services.AddSingleton<AuditEntitiesDbTransactionInterceptor<TPermission>>();
        services.AddAuditEntitiesOptions(options);

        return services;
    }

    /// <summary>
    /// Add AddAuditEntities which includes AuditEntities services
    /// </summary>
    /// <param name="TPermission">Type taht will be set in rule configuration for each entity rule</param>
    /// <param name="TConsumer">The implementation of IAuditEntitiesConsumer interface</param>
    /// <param name="TInstance">The DbContext or any class</param>
    /// <param name="services">The collection of services</param>
    /// <param name="assemblies">The assemblies to scan</param>
    /// <param name="lifetime">The lifetime of the AuditEntities. The default is scoped (per-request in web application)</param>
    /// <param name="filter">Optional filter that allows certain types to be skipped from registration.</param>
    /// <param name="includeInternalTypes">Include internal AuditEntities. The default is false.</param>
    /// <returns>AssemblyScanner</returns>
    public static IServiceCollection AddAuditEntities<TPermission, TConsumer, TInstance>(
        this IServiceCollection services,
        IEnumerable<Assembly> assemblies,
        Action<AuditEntitiesOptions>? options = null,
        ServiceLifetime lifetime = ServiceLifetime.Scoped,
        Func<AssemblyScanner.AssemblyScanResult, bool> filter = null!,
        bool includeInternalTypes = false)
        where TConsumer : class, IAuditEntitiesConsumer<TPermission, TInstance>
        where TInstance : class
    {
        services.AddHttpContextAccessor();
        services.AddAuditEntitiesFromAssemblies<TInstance>(assemblies, lifetime, filter, includeInternalTypes);
        services.AddScoped<IAuditEntitiesService<TPermission, TInstance>, AuditEntitiesService<TPermission, TInstance>>();
        services.AddScoped(typeof(IAuditEntitiesConsumer<TPermission, TInstance>), typeof(TConsumer));
        services.AddAuditEntitiesOptions(options);

        services.AddSingleton<AuditEntitiesSaveInterceptor<TPermission, TInstance>>();
        services.AddSingleton<AuditEntitiesDbTransactionInterceptor<TPermission, TInstance>>();

        return services;
    }

    /// <summary>
    /// Add AddAuditEntities which includes AuditEntities services
    /// </summary>
    /// <param name="TPermission">Type taht will be set in rule configuration for each entity rule</param>
    /// <param name="TConsumer">The implementation of IAuditEntitiesConsumer interface</param>
    /// <param name="TInstance">The DbContext or any class</param>
    /// <param name="services">The collection of services</param>
    /// <param name="assemblies">The assemblies to scan</param>
    /// <param name="lifetime">The lifetime of the AuditEntities. The default is scoped (per-request in web application)</param>
    /// <param name="filter">Optional filter that allows certain types to be skipped from registration.</param>
    /// <param name="includeInternalTypes">Include internal AuditEntities. The default is false.</param>
    /// <returns>AssemblyScanner</returns>
    public static IServiceCollection AddAuditEntities<TPermission, TConsumer, TInstance>(
        this IServiceCollection services,
        Assembly assembly,
        Action<AuditEntitiesOptions>? options = null,
        ServiceLifetime lifetime = ServiceLifetime.Scoped,
        Func<AssemblyScanner.AssemblyScanResult, bool> filter = null!,
        bool includeInternalTypes = false)
        where TConsumer : class, IAuditEntitiesConsumer<TPermission, TInstance>
        where TInstance : class
    {
        services.AddAuditEntities<TPermission, TConsumer, TInstance>([assembly], options, lifetime, filter, includeInternalTypes);

        return services;
    }

    private static IServiceCollection AddAuditEntitiesOptions(this IServiceCollection services, Action<AuditEntitiesOptions>? AuditEntitiesOptions)
    {
        if (AuditEntitiesOptions is null)
        {
            services.Configure<AuditEntitiesOptions>(options =>
            {
                options.AutoOpenTransaction = false;
            });
            return services;
        }

        services.Configure(AuditEntitiesOptions);
        return services;
    }

    /// <summary>
    /// Create AssemblyScanner which includes AuditEntities entity roles in specified assembly
    /// </summary>
    /// <param name="services">The collection of services</param>
    /// <param name="assembly">The assembly to scan</param>
    /// <param name="lifetime">The lifetime of the AuditEntities. The default is scoped (per-request in web application)</param>
    /// <param name="filter">Optional filter that allows certain types to be skipped from registration.</param>
    /// <param name="includeInternalTypes">Include internal AuditEntities. The default is false.</param>
    /// <returns>AssemblyScanner</returns>
    public static AssemblyScanner CreateAuditEntitiesAssemblyScannerFromAssembly(this IServiceCollection services,
        Assembly assembly, ServiceLifetime lifetime = ServiceLifetime.Scoped,
        Func<AssemblyScanner.AssemblyScanResult, bool> filter = null!,
        bool includeInternalTypes = false)
    {
        var assemblyScanner = AssemblyScanner.FindTypeInAssembly(assembly, includeInternalTypes);
        assemblyScanner.ForEach(scanResult => services.AddScanResult(scanResult, lifetime, filter));

        return assemblyScanner;
    }

    /// <summary>
    /// Adds all AuditEntities entity roles from specified assembly
    /// </summary>
    /// <param name="services">The collection of services</param>
    /// <param name="assemblies">The assemblies to scan</param>
    /// <param name="lifetime">The lifetime of the AuditEntities. The default is scoped (per-request in web application)</param>
    /// <param name="filter">Optional filter that allows certain types to be skipped from registration.</param>
    /// <param name="includeInternalTypes">Include internal AuditEntities. The default is false.</param>
    /// <returns></returns>
	public static IServiceCollection AddAuditEntitiesFromAssemblies<TInstance>(this IServiceCollection services,
        IEnumerable<Assembly> assemblies,
        ServiceLifetime lifetime = ServiceLifetime.Scoped,
        Func<AssemblyScanner.AssemblyScanResult, bool> filter = null!,
        bool includeInternalTypes = false)
    {
        ArgumentNullException.ThrowIfNull(assemblies);

        List<AssemblyScanner> scanners = [];
        foreach (var assembly in assemblies)
        {
            var assemblyScanner = AssemblyScanner.FindTypeInAssembly(assembly, includeInternalTypes);
            assemblyScanner.ForEach(scanResult => services.AddScanResult(scanResult, lifetime, filter));
            scanners.Add(assemblyScanner);
        }

        services.AddSingleton<IAuditEntitiesAssemblyProvider<TInstance>>(new AuditEntitiesAssemblyProvider<TInstance>(scanners));

        return services;
    }

    public static DbContextOptionsBuilder UseAuditEntities<TPermission>(this DbContextOptionsBuilder optionsBuilder, IServiceProvider serviceProvider)
    {
        var auditSaveInterceptor = serviceProvider.GetRequiredService<AuditEntitiesSaveInterceptor<TPermission>>();
        var auditDbTransactionInterceptor = serviceProvider.GetRequiredService<AuditEntitiesDbTransactionInterceptor<TPermission>>();

        optionsBuilder.AddInterceptors(auditSaveInterceptor);
        optionsBuilder.AddInterceptors(auditDbTransactionInterceptor);

        return optionsBuilder;
    }

    public static DbContextOptionsBuilder UseAuditEntities<TPermission, TInstance>(this DbContextOptionsBuilder optionsBuilder, IServiceProvider serviceProvider)
        where TInstance : class
    {
        var auditSaveInterceptor = serviceProvider.GetRequiredService<AuditEntitiesSaveInterceptor<TPermission, TInstance>>();
        var auditDbTransactionInterceptor = serviceProvider.GetRequiredService<AuditEntitiesDbTransactionInterceptor<TPermission, TInstance>>();

        optionsBuilder.AddInterceptors(auditSaveInterceptor);
        optionsBuilder.AddInterceptors(auditDbTransactionInterceptor);

        return optionsBuilder;
    }

    /// <summary>
    /// Adds all AuditEntities entity roles from specified assembly
    /// </summary>
    /// <param name="services">The collection of services</param>
    /// <param name="assembly">The assembly to scan</param>
    /// <param name="lifetime">The lifetime of the AuditEntities. The default is scoped (per-request in web application)</param>
    /// <param name="filter">Optional filter that allows certain types to be skipped from registration.</param>
    /// <param name="includeInternalTypes">Include internal AuditEntities. The default is false.</param>
    /// <returns></returns>
    public static IServiceCollection AddAuditEntitiesFromAssembly<TInstance>(this IServiceCollection services, Assembly assembly, ServiceLifetime lifetime = ServiceLifetime.Scoped, Func<AssemblyScanner.AssemblyScanResult, bool> filter = null!, bool includeInternalTypes = false)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        AssemblyScanner
            .FindTypeInAssembly(assembly, includeInternalTypes)
            .AddAssemblyToProvider<TInstance>(services)
            .ForEach(scanResult => services.AddScanResult(scanResult, lifetime, filter));

        return services;
    }

    /// <summary>
    /// Helper method to register a AuditEntities from an AssemblyScanner result
    /// </summary>
    /// <param name="services">The collection of services</param>
    /// <param name="scanResult">The scan result</param>
    /// <param name="lifetime">The lifetime of the AuditEntities. The default is scoped (per-request in web applications)</param>
    /// <param name="filter">Optional filter that allows certain types to be skipped from registration.</param>
    /// <returns></returns>
    private static IServiceCollection AddScanResult(this IServiceCollection services, AssemblyScanner.AssemblyScanResult scanResult, ServiceLifetime lifetime, Func<AssemblyScanner.AssemblyScanResult, bool> filter)
    {
        bool shouldRegister = filter?.Invoke(scanResult) ?? true;
        if (shouldRegister)
        {
            //Register as interface
            services.TryAddEnumerable(
                new ServiceDescriptor(
                    serviceType: scanResult.InterfaceType,
                    implementationType: scanResult.RuleType,
                    lifetime: lifetime));

            //Register as self
            services.TryAdd(
                new ServiceDescriptor(
                    serviceType: scanResult.RuleType,
                    implementationType: scanResult.RuleType,
                    lifetime: lifetime));
        }

        return services;
    }

    private static AssemblyScanner AddAssemblyToProvider<TInstance>(this AssemblyScanner assemblyScans, IServiceCollection services)
    {
        services.AddSingleton<IAuditEntitiesAssemblyProvider<TInstance>>(new AuditEntitiesAssemblyProvider<TInstance>([assemblyScans]));
        return assemblyScans;
    }
}
