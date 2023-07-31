namespace PlugIn.Core.Ioc;
public interface IServiceRegistry
{
    IServiceRegistry RegisterInstance(Type type, object instance);

    IServiceRegistry RegisterSingleton(Type type);
    IServiceRegistry RegisterSingleton(Type from, Type to);
    IServiceRegistry RegisterSingleton(Type type, Func<object> factoryMethod);
    IServiceRegistry RegisterSingleton(Type type, Func<IServiceProvider, object> factoryMethod);
    IServiceRegistry RegisterManySingleton(Type type, params Type[] serviceTypes);

    IServiceRegistry RegisterTransient(Type type);
    IServiceRegistry RegisterTransient(Type from, Type to);
    IServiceRegistry RegisterTransient(Type type, Func<object> factoryMethod);
    IServiceRegistry RegisterTransient(Type type, Func<IServiceProvider, object> factoryMethod);
    IServiceRegistry RegisterManyTransient(Type type, params Type[] serviceTypes);

    IServiceRegistry RegisterScoped(Type type);
    IServiceRegistry RegisterScoped(Type from, Type to);
    IServiceRegistry RegisterScoped(Type type, Func<object> factoryMethod);
    IServiceRegistry RegisterScoped(Type type, Func<IServiceProvider, object> factoryMethod);
    IServiceRegistry RegisterManyScoped(Type type, params Type[] serviceTypes);

    bool IsRegistered(Type type);
}
