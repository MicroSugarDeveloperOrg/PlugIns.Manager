using PlugIn.Core.Enum;
using PlugIn.Core.Events;
using PlugIn.Core.Exceptions;
using System.Collections.Concurrent;

namespace PlugIn.Core.Modularity;


public partial class PlugInManager : IPlugInManager, IDisposable
{
    public PlugInManager(IModuleInitializer moduleInitializer, IModuleCatalog moduleCatalog)
    {
        _moduleCatalog = moduleCatalog ?? throw new ArgumentNullException(nameof(moduleCatalog));
        _moduleInitializer = moduleInitializer ?? throw new ArgumentNullException(nameof(moduleInitializer));
        _typeLoaders = new List<IModuleTypeLoader> { new FileModuleTypeLoader() };
    }

    readonly IModuleCatalog _moduleCatalog;

    readonly IModuleInitializer _moduleInitializer;
    IEnumerable<IModuleTypeLoader> _typeLoaders;
    readonly HashSet<IModuleTypeLoader> _subscribedToModuleTypeLoaders = new();
    readonly ConcurrentDictionary<IModuleInfo, IModule> _mapModuleInstances = new();

    protected IModuleCatalog ModuleCatalog => _moduleCatalog;
    public IEnumerable<IModuleInfo> Modules => ModuleCatalog.Modules;

    public event EventHandler<LoadModuleCompletedEventArgs>? LoadModuleCompleted;
    public event EventHandler<ModuleDownloadProgressChangedEventArgs>? ModuleDownloadProgressChanged;
    public event EventHandler<ModuleRunningEventArgs>? ModuleRunning;

    private void RaiseModuleDownloadProgressChanged(ModuleDownloadProgressChangedEventArgs e)
    {
        ModuleDownloadProgressChanged?.Invoke(this, e);
    }

    private void RaiseLoadModuleCompleted(IModuleInfo moduleInfo, Exception? error)
    {
        RaiseLoadModuleCompleted(new LoadModuleCompletedEventArgs(moduleInfo, error));
    }

    private void RaiseLoadModuleCompleted(LoadModuleCompletedEventArgs e)
    {
        LoadModuleCompleted?.Invoke(this, e);
    }

    void RaiseModuleRunning(IModuleInfo moduleInfo, Exception? error)
    {
        RaiseModuleRunning(new ModuleRunningEventArgs(moduleInfo, error));
    }

    void RaiseModuleRunning(ModuleRunningEventArgs e)
    {
        ModuleRunning?.Invoke(this, e);
    }

    public bool Initialize()
    {
        ModuleCatalog.Initialize();
        LoadModulesWhenAvailable();
        return true;
    }

    public bool Run(IServiceProvider serviceProvider)
    {
        foreach (var item in _mapModuleInstances)
        {
            if (item.Key.State == ModuleState.Running)
                continue;

            if (_moduleInitializer.Run(serviceProvider, item.Value))
            {
                item.Key.State = ModuleState.Running;
                RaiseModuleRunning(item.Key, null);
            }
        }

        return true;
    }

    public bool ReloadModules()
    {
        throw new NotImplementedException();
    }

    public void LoadModule(Type moduleType)
    {
        var module = ModuleCatalog.Modules.Where(m => m.ModuleName == moduleType.Name);
        var modulesToLoad = ModuleCatalog.CompleteListWithDependencies(module);
        LoadModuleTypes(modulesToLoad);
    }

    public void LoadModule(string moduleName)
    {
        var module = ModuleCatalog.Modules.Where(m => m.ModuleName == moduleName);
        if (module == null || module.Count() != 1)
            throw new ModuleNotFoundException(moduleName, $"ModuleNotFound ModuleName:{moduleName}");

        var modulesToLoad = ModuleCatalog.CompleteListWithDependencies(module);
        LoadModuleTypes(modulesToLoad);
    }

    protected virtual bool ModuleNeedsRetrieval(IModuleInfo moduleInfo)
    {
        if (moduleInfo == null)
            throw new ArgumentNullException(nameof(moduleInfo));

        if (moduleInfo.State == ModuleState.NotStarted)
        {
            bool isAvailable = moduleInfo.ModuleType != null;
            if (isAvailable)
                moduleInfo.State = ModuleState.ReadyForInitialization;

            return !isAvailable;
        }

        return false;
    }

    private void LoadModulesWhenAvailable()
    {
        var whenAvailableModules = ModuleCatalog.Modules.Where(m => m.InitializationMode == InitializationMode.WhenAvailable);
        var modulesToLoadTypes = ModuleCatalog.CompleteListWithDependencies(whenAvailableModules);
        if (modulesToLoadTypes != null)
        {
            LoadModuleTypes(modulesToLoadTypes);
        }
    }

    private void LoadModuleTypes(IEnumerable<IModuleInfo> moduleInfos)
    {
        if (moduleInfos == null)
        {
            return;
        }

        foreach (var moduleInfo in moduleInfos)
        {
            if (moduleInfo.State == ModuleState.NotStarted)
            {
                if (ModuleNeedsRetrieval(moduleInfo))
                    BeginRetrievingModule(moduleInfo);
                else
                    moduleInfo.State = ModuleState.ReadyForInitialization;
            }
        }

        LoadModulesThatAreReadyForLoad();
    }

    /// <summary>
    /// Loads the modules that are not initialized and have their dependencies loaded.
    /// </summary>
    protected virtual void LoadModulesThatAreReadyForLoad()
    {
        bool keepLoading = true;
        while (keepLoading)
        {
            keepLoading = false;
            var availableModules = ModuleCatalog.Modules.Where(m => m.State == ModuleState.ReadyForInitialization);

            foreach (var moduleInfo in availableModules)
            {
                if ((moduleInfo.State != ModuleState.Initialized) && (AreDependenciesLoaded(moduleInfo)))
                {
                    moduleInfo.State = ModuleState.Initializing;
                    InitializeModule(moduleInfo);
                    keepLoading = true;
                    break;
                }
            }
        }
    }

    private void BeginRetrievingModule(IModuleInfo moduleInfo)
    {
        var moduleInfoToLoadType = moduleInfo;
        IModuleTypeLoader moduleTypeLoader = GetTypeLoaderForModule(moduleInfoToLoadType);
        moduleInfoToLoadType.State = ModuleState.LoadingTypes;

        // Delegate += works differently between SL and WPF.
        // We only want to subscribe to each instance once.
        if (!_subscribedToModuleTypeLoaders.Contains(moduleTypeLoader))
        {
            moduleTypeLoader.ModuleDownloadProgressChanged += IModuleTypeLoader_ModuleDownloadProgressChanged;
            moduleTypeLoader.LoadModuleCompleted += IModuleTypeLoader_LoadModuleCompleted;
            _subscribedToModuleTypeLoaders.Add(moduleTypeLoader);
        }

        moduleTypeLoader.LoadModuleType(moduleInfo);
    }

    private void IModuleTypeLoader_ModuleDownloadProgressChanged(object sender, ModuleDownloadProgressChangedEventArgs e)
    {
        RaiseModuleDownloadProgressChanged(e);
    }

    private void IModuleTypeLoader_LoadModuleCompleted(object sender, LoadModuleCompletedEventArgs e)
    {
        if (e.Error == null)
        {
            if ((e.ModuleInfo.State != ModuleState.Initializing) && (e.ModuleInfo.State != ModuleState.Initialized))
            {
                e.ModuleInfo.State = ModuleState.ReadyForInitialization;
            }

            LoadModulesThatAreReadyForLoad();
        }
        else
        {
            RaiseLoadModuleCompleted(e);

            if (!e.IsErrorHandled)
                HandleModuleTypeLoadingError(e.ModuleInfo, e.Error);
        }
    }

    protected virtual void HandleModuleTypeLoadingError(IModuleInfo moduleInfo, Exception exception)
    {
        if (moduleInfo == null)
            throw new ArgumentNullException(nameof(moduleInfo));

        if (!(exception is ModuleTypeLoadingException moduleTypeLoadingException))
            moduleTypeLoadingException = new ModuleTypeLoadingException(moduleInfo.ModuleName, exception.Message, exception);

        throw moduleTypeLoadingException;
    }

    private bool AreDependenciesLoaded(IModuleInfo moduleInfo)
    {
        var requiredModules = ModuleCatalog.GetDependentModules(moduleInfo);
        if (requiredModules == null)
            return true;

        int notReadyRequiredModuleCount =
            requiredModules.Count(requiredModule => requiredModule.State != ModuleState.Initialized);

        return notReadyRequiredModuleCount == 0;
    }

    private IModuleTypeLoader GetTypeLoaderForModule(IModuleInfo moduleInfo)
    {
        foreach (IModuleTypeLoader typeLoader in ModuleTypeLoaders)
        {
            if (typeLoader.CanLoadModuleType(moduleInfo))
                return typeLoader;
        }

        throw new ModuleTypeLoaderNotFoundException(moduleInfo.ModuleName, $"NoRetrieverCanRetrieveModule ModuleName:{moduleInfo.ModuleName}", null);
    }

    private void InitializeModule(IModuleInfo moduleInfo)
    {
        if (moduleInfo.State == ModuleState.Initializing)
        {
            IModule? moduleInstance = _moduleInitializer.Initialize(moduleInfo);
            moduleInfo.State = ModuleState.Initialized;
            if (moduleInstance != null)
            {
                _mapModuleInstances.AddOrUpdate(moduleInfo, moduleInstance, (key, value)=> value);
                RaiseLoadModuleCompleted(moduleInfo, null);
            }
        }
    }

    public virtual IEnumerable<IModuleTypeLoader> ModuleTypeLoaders
    {
        get => _typeLoaders;
        set => _typeLoaders = value;
    }

    #region Implementation of IDisposable

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <remarks>Calls <see cref="Dispose(bool)"/></remarks>.
    /// <filterpriority>2</filterpriority>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the associated <see cref="IModuleTypeLoader"/>s.
    /// </summary>
    /// <param name="disposing">When <see langword="true"/>, it is being called from the Dispose method.</param>
    protected virtual void Dispose(bool disposing)
    {
        foreach (IModuleTypeLoader typeLoader in ModuleTypeLoaders)
        {
            if (typeLoader is IDisposable disposableTypeLoader)
            {
                disposableTypeLoader.Dispose();
            }
        }
    }

    #endregion
}
