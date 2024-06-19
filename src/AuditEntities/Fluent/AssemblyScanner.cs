namespace AuditEntities.Fluent;

using AuditEntities.Fluent.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/// <summary>
/// Class that can be used to find all the types from a collection of types.
/// </summary>
public class AssemblyScanner : IEnumerable<AssemblyScanner.AssemblyScanResult>
{
    readonly IEnumerable<Type> _types;

    /// <summary>
    /// Creates a scanner that works on a sequence of types.
    /// </summary>
    public AssemblyScanner(IEnumerable<Type> types)
    {
        _types = types;
    }

    /// <summary>
    /// Finds all the types in the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly to scan</param>
    /// <param name="includeInternalTypes">Whether to include internal types in the search as well as public types. The default is false.</param>
    public static AssemblyScanner FindTypeInAssembly(Assembly assembly, bool includeInternalTypes = false)
    {
        return new AssemblyScanner(includeInternalTypes ? assembly.GetTypes() : assembly.GetExportedTypes());
    }

    /// <summary>
    /// Finds all the types in the specified assemblies.
    /// </summary>
    /// <param name="assemblies">The assemblies to scan</param>
    /// <param name="includeInternalTypes">Whether to include internal types as well as public types. The default is false.</param>
    public static AssemblyScanner FindTypeInAssemblies(IEnumerable<Assembly> assemblies, bool includeInternalTypes = false)
    {
        var types = assemblies.SelectMany(x => includeInternalTypes ? x.GetTypes() : x.GetExportedTypes()).Distinct();
        return new AssemblyScanner(types);
    }

    private IEnumerable<AssemblyScanResult> Execute()
    {
        var query = GetTypes(typeof(IEntityRule<,>)).ToList();
        query.AddRange(GetTypes(typeof(IEntityRule<,,>)));

        return query;
    }

    private IEnumerable<AssemblyScanResult> GetTypes(Type openGenericType)
    {
        return from type in _types
               where !type.IsAbstract && !type.IsGenericTypeDefinition
               let interfaces = type.GetInterfaces()
               let genericInterfaces = interfaces.Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == openGenericType)
               let matchingInterface = genericInterfaces.FirstOrDefault()
               where matchingInterface != null
               select new AssemblyScanResult(matchingInterface, type);
    }

    /// <summary>
    /// Performs the specified action to all of the assembly scan results.
    /// </summary>
    public void ForEach(Action<AssemblyScanResult> action)
    {
        foreach (AssemblyScanResult result in this)
        {
            action(result);
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
    /// </returns>
    /// <filterpriority>1</filterpriority>
    public IEnumerator<AssemblyScanResult> GetEnumerator()
    {
        return Execute().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Result of performing a scan.
    /// </summary>
    public class AssemblyScanResult
    {
        /// <summary>
        /// Creates an instance of an AssemblyScanResult.
        /// </summary>
        public AssemblyScanResult(Type interfaceType, Type ruleType)
        {
            InterfaceType = interfaceType;
            RuleType = ruleType;
        }

        public Type InterfaceType { get; private set; }

        public Type RuleType { get; private set; }
    }
}
