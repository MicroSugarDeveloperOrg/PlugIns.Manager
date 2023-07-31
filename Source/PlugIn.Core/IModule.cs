using PlugIn.Core.Ioc;

namespace PlugIn.Core;
public interface IModule
{
    /// <summary>
    /// Used to register types with the container that will be used by your application.
    /// </summary>
    void RegisterTypes(IServiceRegistry containerRegistry);

    /// <summary>
    /// Notifies the module that it has been initialized.
    /// </summary>
    bool OnInitialized(IServiceProvider containerProvider);

    //bool Run(IServiceProvider containerProvider);
}