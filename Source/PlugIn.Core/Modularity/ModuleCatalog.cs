namespace PlugIn.Core.Modularity;
public class ModuleCatalog : ModuleCatalogBase
{
    public ModuleCatalog()
        : base()
    {

    }

    public ModuleCatalog(IEnumerable<IModuleInfo> modules)
        : base(modules)
    {

    }

    protected virtual string GetFileAbsoluteUri(string filePath)
    {
        UriBuilder uriBuilder = new UriBuilder();
        uriBuilder.Host = string.Empty;
        uriBuilder.Scheme = Uri.UriSchemeFile;
        uriBuilder.Path = Path.GetFullPath(filePath);
        Uri fileUri = uriBuilder.Uri;

        return fileUri.ToString();
    }

    protected override void InnerLoad()
    {
        
    }
}
