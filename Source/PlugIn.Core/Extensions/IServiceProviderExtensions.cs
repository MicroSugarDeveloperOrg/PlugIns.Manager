namespace PlugIn.Core.Extensions;
public static class IServiceProviderExtensions
{
    public static T? GetService<T>(this IServiceProvider serviceProvider)
    {
        return (T?)serviceProvider.GetService(typeof(T));
    }

    public static object GetRequiredService(this IServiceProvider serviceProvider, Type type)
    {
        var tValue = serviceProvider.GetService(type);
        if (tValue is null)
            throw new ArgumentNullException($"Value is null,type is {type}");

        return tValue;
    }

    public static T GetRequiredService<T>(this IServiceProvider serviceProvider)
    {
        var tValue = serviceProvider.GetService<T>();
        if (tValue is null)
            throw new ArgumentNullException($"Value is null,type is {typeof(T)}");

        return tValue;
    }
}
