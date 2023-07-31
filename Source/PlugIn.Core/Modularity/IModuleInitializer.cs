namespace PlugIn.Core.Modularity;
public interface IModuleInitializer
{
    /// <summary>
    /// Initializes the specified module.
    /// </summary>
    /// <param name="moduleInfo">The module to initialize</param>
    IModule? Initialize(IModuleInfo moduleInfo);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="module"></param>
    /// <returns></returns>
    bool Run(IServiceProvider serviceProvider, IModule module);

}