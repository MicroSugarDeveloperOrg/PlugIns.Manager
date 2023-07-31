using PlugIn.Core.Ioc;

namespace PlugIn.Core.Extensions;
public static class IServiceRegistryExtensions
{
    public static IServiceRegistry RegisterInstance<T>(this IServiceRegistry serviceRegistry, T instance) where T : class
        => serviceRegistry.RegisterInstance(typeof(T), instance);


    public static IServiceRegistry RegisterSingleton<T>(this IServiceRegistry serviceRegistry) where T : class
        => serviceRegistry.RegisterSingleton(typeof(T), typeof(T));

    public static IServiceRegistry RegisterSingleton<TFrom, TTo>(this IServiceRegistry serviceRegistry) where TTo : class, TFrom
        => serviceRegistry.RegisterSingleton(typeof(TFrom), typeof(TTo));

    public static IServiceRegistry RegisterSingleton<T>(this IServiceRegistry serviceRegistry, Func<T> factoryMethod) where T : class
        => serviceRegistry.RegisterSingleton(typeof(T), factoryMethod);

    public static IServiceRegistry RegisterSingleton<T>(this IServiceRegistry serviceRegistry, Func<IServiceProvider, T> factoryMethod) where T : class
        => serviceRegistry.RegisterSingleton(typeof(T), factoryMethod);


    public static IServiceRegistry RegisterTransient<T>(this IServiceRegistry serviceRegistry) where T : class
        => serviceRegistry.RegisterTransient(typeof(T), typeof(T));

    public static IServiceRegistry RegisterTransient<TFrom, TTo>(this IServiceRegistry serviceRegistry) where TTo : class, TFrom
        => serviceRegistry.RegisterTransient(typeof(TFrom), typeof(TTo));

    public static IServiceRegistry RegisterTransient<T>(this IServiceRegistry serviceRegistry, Func<T> factoryMethod) where T : class
        => serviceRegistry.RegisterTransient(typeof(T), factoryMethod);

    public static IServiceRegistry RegisterTransient<T>(this IServiceRegistry serviceRegistry, Func<IServiceProvider, T> factoryMethod) where T : class
        => serviceRegistry.RegisterTransient(typeof(T), factoryMethod);


    public static IServiceRegistry RegisterScoped<T>(this IServiceRegistry serviceRegistry) where T : class
        => serviceRegistry.RegisterScoped(typeof(T), typeof(T));

    public static IServiceRegistry RegisterScoped<TFrom, TTo>(this IServiceRegistry serviceRegistry) where TTo : class, TFrom
        => serviceRegistry.RegisterScoped(typeof(TFrom), typeof(TTo));

    public static IServiceRegistry RegisterScoped<T>(this IServiceRegistry serviceRegistry, Func<T> factoryMethod) where T : class
        => serviceRegistry.RegisterScoped(typeof(T), factoryMethod);

    public static IServiceRegistry RegisterScoped<T>(this IServiceRegistry serviceRegistry, Func<IServiceProvider, T> factoryMethod) where T : class
        => serviceRegistry.RegisterScoped(typeof(T), factoryMethod);
}