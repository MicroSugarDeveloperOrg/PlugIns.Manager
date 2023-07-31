using System.Globalization;
using System.Reflection;

namespace PlugIn.Core.Extensions;
public static class ActivatorExtensions
{
    private const BindingFlags ConstructorDefault = BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance;

    public static object? CreateInstanceFrom(string assemblyFile, string typeName) => CreateInstanceFrom(assemblyFile, typeName, false, ConstructorDefault, null, null, null, null);

    public static object? CreateInstanceFrom(string assemblyFile, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder? binder, object?[]? args, CultureInfo? culture, object?[]? activationAttributes)
    {
        Assembly assembly = Assembly.LoadFrom(assemblyFile);
        Type t = assembly.GetType(typeName, throwOnError: true, ignoreCase)!;

        object? o = Activator.CreateInstance(t, bindingAttr, binder, args, culture, activationAttributes);
        return o;
    }
}
