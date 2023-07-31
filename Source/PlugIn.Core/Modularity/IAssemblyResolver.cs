namespace PlugIn.Core.Modularity;
public interface IAssemblyResolver
{
    /// <summary>
    /// Load an assembly when it's required by the application. 
    /// </summary>
    /// <param name="assemblyFilePath"></param>
    void LoadAssemblyFrom(string assemblyFilePath);
}