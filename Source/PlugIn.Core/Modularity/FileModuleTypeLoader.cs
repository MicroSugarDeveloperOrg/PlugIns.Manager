using PlugIn.Core.Events;

namespace PlugIn.Core.Modularity;
public class FileModuleTypeLoader : IModuleTypeLoader, IDisposable
{
    public FileModuleTypeLoader()
        : this(new AssemblyResolver())
    {
    }

    public FileModuleTypeLoader(IAssemblyResolver assemblyResolver)
    {
        _assemblyResolver = assemblyResolver;
    }

    private const string RefFilePrefix = "file://";

    readonly IAssemblyResolver _assemblyResolver;
    readonly HashSet<Uri> _downloadedUris = new HashSet<Uri>();

    public event EventHandler<LoadModuleCompletedEventArgs>? LoadModuleCompleted;
    public event EventHandler<ModuleDownloadProgressChangedEventArgs>? ModuleDownloadProgressChanged;

    private void RaiseModuleDownloadProgressChanged(IModuleInfo moduleInfo, long bytesReceived, long totalBytesToReceive)
    {
        RaiseModuleDownloadProgressChanged(new ModuleDownloadProgressChangedEventArgs(moduleInfo, bytesReceived, totalBytesToReceive));
    }

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

    public bool CanLoadModuleType(IModuleInfo moduleInfo)
    {
        if (moduleInfo == null)
            throw new ArgumentNullException(nameof(moduleInfo));

        return moduleInfo.Ref != null && moduleInfo.Ref.StartsWith(RefFilePrefix, StringComparison.Ordinal);
    }

    public void LoadModuleType(IModuleInfo moduleInfo)
    {
        if (moduleInfo == null)
            throw new ArgumentNullException(nameof(moduleInfo));

        try
        {
            Uri uri = new Uri(moduleInfo.Ref, UriKind.RelativeOrAbsolute);

            if (IsSuccessfullyDownloaded(uri))
                RaiseLoadModuleCompleted(moduleInfo, null);
            else
            {
                string path = uri.LocalPath;

                long fileSize = -1L;
                if (File.Exists(path))
                {
                    FileInfo fileInfo = new FileInfo(path);
                    fileSize = fileInfo.Length;
                }

                RaiseModuleDownloadProgressChanged(moduleInfo, 0, fileSize);

                if (!string.IsNullOrWhiteSpace(moduleInfo.Ref))
                    _assemblyResolver.LoadAssemblyFrom(moduleInfo.Ref!);

                RaiseModuleDownloadProgressChanged(moduleInfo, fileSize, fileSize);

                RecordDownloadSuccess(uri);

                RaiseLoadModuleCompleted(moduleInfo, null);
            }
        }
        catch (Exception ex)
        {
            RaiseLoadModuleCompleted(moduleInfo, ex);
        }
    }

    private bool IsSuccessfullyDownloaded(Uri uri)
    {
        lock (_downloadedUris)
            return _downloadedUris.Contains(uri);
    }

    private void RecordDownloadSuccess(Uri uri)
    {
        lock (_downloadedUris)
            _downloadedUris.Add(uri);
    }

    #region Implementation of IDisposable

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (this._assemblyResolver is IDisposable disposableResolver)
            disposableResolver.Dispose();
    }

    #endregion
}