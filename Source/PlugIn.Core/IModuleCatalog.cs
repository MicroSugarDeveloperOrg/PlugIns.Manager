namespace PlugIn.Core;
public interface IModuleCatalog
{
    IEnumerable<IModuleInfo> Modules { get; }
    void Initialize();
    IModuleCatalog AddModule(IModuleInfo moduleInfo);
    IEnumerable<IModuleInfo> GetDependentModules(IModuleInfo moduleInfo);
    IEnumerable<IModuleInfo> CompleteListWithDependencies(IEnumerable<IModuleInfo> modules);
}
