namespace PlugIn.Core.Events;
public class LoadModuleCompletedEventArgs : EventArgs
{
    public LoadModuleCompletedEventArgs(IModuleInfo moduleInfo, Exception? error)
    {
        if (moduleInfo == null)
            throw new ArgumentNullException(nameof(moduleInfo));

        ModuleInfo = moduleInfo;
        Error = error;
    }

    public IModuleInfo ModuleInfo { get; private set; }
 
    public Exception? Error { get; private set; }
 
    public bool IsErrorHandled { get; set; }
}