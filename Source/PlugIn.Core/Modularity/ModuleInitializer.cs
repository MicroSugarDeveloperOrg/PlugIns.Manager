using PlugIn.Core.Exceptions;
using PlugIn.Core.Ioc;

namespace PlugIn.Core.Modularity;
public class ModuleInitializer : IModuleInitializer
{
    //public ModuleInitializer(IIocStorage iocStorage)
    //{
    //    _iocStorage = iocStorage ?? throw new ArgumentNullException(nameof(iocStorage)); 
    //}

    //readonly IIocStorage _iocStorage; 

    public ModuleInitializer(IServiceRegistry serviceRegistry)
    {
        _serviceRegistry = serviceRegistry ?? throw new ArgumentNullException(nameof(serviceRegistry));
    }

    readonly IServiceRegistry _serviceRegistry;


    public IModule? Initialize(IModuleInfo moduleInfo)
    {
        if (moduleInfo == null)
            throw new ArgumentNullException(nameof(moduleInfo));

        IModule? moduleInstance = default;
        try
        {
            moduleInstance = CreateModule(moduleInfo);
            if (moduleInstance != null)
            {
                moduleInstance.RegisterTypes(_serviceRegistry);
                //moduleInstance.RegisterTypes(_iocStorage.BuildRegistry());
                //moduleInstance.OnInitialized(_iocStorage.BuildProvider());
            }
        }
        catch (Exception ex)
        {
            HandleModuleInitializationError(moduleInfo, moduleInstance?.GetType().Assembly.FullName, ex);
        }

        return moduleInstance;
    }


    public bool Run(IServiceProvider serviceProvider, IModule module)
    {
        if (module is null)
            return false;

        if (serviceProvider is null)
            return false;

        return module.OnInitialized(serviceProvider);
    }

    public virtual void HandleModuleInitializationError(IModuleInfo moduleInfo, string? assemblyName, Exception exception)
    {
        if (moduleInfo == null)
            throw new ArgumentNullException(nameof(moduleInfo));

        if (exception == null)
            throw new ArgumentNullException(nameof(exception));

        Exception moduleException;

        if (exception is ModuleInitializeException)
        {
            moduleException = exception;
        }
        else
        {
            if (!string.IsNullOrEmpty(assemblyName))
            {
                moduleException = new ModuleInitializeException(moduleInfo.ModuleName!, assemblyName!, exception.Message, exception);
            }
            else
            {
                moduleException = new ModuleInitializeException(moduleInfo.ModuleName!, exception.Message, exception);
            }
        }

        throw moduleException;
    }

    /// <summary>
    /// Uses the container to resolve a new <see cref="IModule"/> by specifying its <see cref="Type"/>.
    /// </summary>
    /// <param name="moduleInfo">The module to create.</param>
    /// <returns>A new instance of the module specified by <paramref name="moduleInfo"/>.</returns>
    protected virtual IModule CreateModule(IModuleInfo moduleInfo)
    {
        if (moduleInfo == null)
            throw new ArgumentNullException(nameof(moduleInfo));

        return CreateModule(moduleInfo.ModuleType);
    }

    /// <summary>
    /// Uses the container to resolve a new <see cref="IModule"/> by specifying its <see cref="Type"/>.
    /// </summary>
    /// <param name="typeName">The type name to resolve. This type must implement <see cref="IModule"/>.</param>
    /// <returns>A new instance of <paramref name="typeName"/>.</returns>
    protected virtual IModule CreateModule(string typeName)
    {
        Type moduleType = Type.GetType(typeName);
        if (moduleType == null)
            throw new ModuleInitializeException($"FailedToGetType TypeName:{typeName}");

        var typeObject = Activator.CreateInstance(moduleType, true);
        if (typeObject is null)
            throw new ModuleInitializeException($"CreateInstance failed, Type is {moduleType}");

        return (IModule)typeObject;
        //return (IModule)_iocStorage.BuildProvider().GetRequiredService(moduleType);
    }

    protected virtual IModule CreateModule(Type? type)
    {
        if (type == null)
            throw new ModuleInitializeException("Type is null");

        var typeObject = Activator.CreateInstance(type, true);
        if (typeObject is null)
            throw new ModuleInitializeException($"CreateInstance failed, Type is {type}");

        return (IModule)typeObject;

        //_iocStorage.BuildRegistry().RegisterSingleton(type);

        // return (IModule)_iocStorage.BuildProvider().GetRequiredService(type);
    }
}