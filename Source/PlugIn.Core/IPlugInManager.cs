using PlugIn.Core.Events;

namespace PlugIn.Core;
public interface IPlugInManager
{
    IEnumerable<IModuleInfo> Modules { get; }

    event EventHandler<ModuleDownloadProgressChangedEventArgs>? ModuleDownloadProgressChanged;
    event EventHandler<LoadModuleCompletedEventArgs>? LoadModuleCompleted;
    event EventHandler<ModuleRunningEventArgs>? ModuleRunning;

    bool Initialize();
    bool Run(IServiceProvider serviceProvider);
    bool ReloadModules();
    void LoadModule(string moduleName);
    void LoadModule(Type moduleType);
}
