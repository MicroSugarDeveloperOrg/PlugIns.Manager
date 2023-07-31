using PlugIn.Core.Events;

namespace PlugIn.Core.Modularity;
public interface IModuleTypeLoader
{
    bool CanLoadModuleType(IModuleInfo moduleInfo);

    void LoadModuleType(IModuleInfo moduleInfo);

    event EventHandler<ModuleDownloadProgressChangedEventArgs> ModuleDownloadProgressChanged;

    event EventHandler<LoadModuleCompletedEventArgs> LoadModuleCompleted;
}