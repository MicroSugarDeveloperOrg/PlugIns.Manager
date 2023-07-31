using PlugIn.Core.Enum;

namespace PlugIn.Core.Extensions;
public static class IPlugInManagerExtensions
{
    /// <summary>
    /// Checks to see if the <see cref="IModule"/> exists in the <see cref="IModuleCatalog.Modules"/>  
    /// </summary>
    /// <returns><c>true</c> if the Module exists.</returns>
    /// <param name="manager">The <see cref="IPlugInManager"/>.</param>
    /// <typeparam name="T">The <see cref="IModule"/> to check for.</typeparam>
    public static bool ModuleExists<T>(this IPlugInManager manager)
        where T : IModule =>
        manager.Modules.Any(mi => mi.ModuleTypeString == typeof(T).AssemblyQualifiedName);

    /// <summary>
    /// Exists the specified catalog and name.
    /// </summary>
    /// <returns><c>true</c> if the Module exists.</returns>
    /// <param name="catalog">Catalog.</param>
    /// <param name="name">Name.</param>
    public static bool ModuleExists(this IPlugInManager catalog, string name) =>
        catalog.Modules.Any(module => module.ModuleName == name);

    /// <summary>
    /// Gets the current <see cref="ModuleState"/> of the <see cref="IModule"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="IModule"/> to check.</typeparam>
    /// <param name="manager">The <see cref="IPlugInManager"/>.</param>
    /// <returns></returns>
    public static ModuleState GetModuleState<T>(this IPlugInManager manager)
        where T : IModule =>
        manager.Modules.FirstOrDefault(mi => mi.ModuleTypeString == typeof(T).AssemblyQualifiedName).State;

    /// <summary>
    /// Gets the current <see cref="ModuleState"/> of the <see cref="IModule"/>.
    /// </summary>
    /// <param name="manager">The <see cref="IPlugInManager"/>.</param>
    /// <param name="name">Name.</param>
    /// <returns></returns>
    public static ModuleState GetModuleState(this IPlugInManager manager, string name) =>
        manager.Modules.FirstOrDefault(module => module.ModuleName == name).State;

    /// <summary>
    /// Checks to see if the <see cref="IModule"/> is already initialized. 
    /// </summary>
    /// <returns><c>true</c>, if initialized, <c>false</c> otherwise.</returns>
    /// <param name="manager">The <see cref="IPlugInManager"/>.</param>
    /// <typeparam name="T">The <see cref="IModule"/> to check.</typeparam>
    public static bool IsModuleInitialized<T>(this IPlugInManager manager)
        where T : IModule =>
        manager.Modules.FirstOrDefault(mi => mi.ModuleTypeString == typeof(T).AssemblyQualifiedName)?.State == ModuleState.Initialized;

    /// <summary>
    /// Checks to see if the <see cref="IModule"/> is already initialized. 
    /// </summary>
    /// <returns><c>true</c>, if initialized, <c>false</c> otherwise.</returns>
    /// <param name="manager">The <see cref="IPlugInManager"/>.</param>
    /// <param name="name">Name.</param>
    public static bool IsModuleInitialized(this IPlugInManager manager, string name) =>
        manager.Modules.FirstOrDefault(module => module.ModuleName == name)?.State == ModuleState.Initialized;

    /// <summary>
    /// Loads and initializes the module in the <see cref="IModuleCatalog"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="IModule"/> to load.</typeparam>
    /// <param name="manager">The <see cref="IPlugInManager"/>.</param>
    public static void LoadModule<T>(this IPlugInManager manager)
        where T : IModule =>
        manager.LoadModule(typeof(T));
}