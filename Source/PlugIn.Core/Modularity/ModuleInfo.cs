using PlugIn.Core.Enum;

namespace PlugIn.Core.Modularity;

[Serializable]
public class ModuleInfo : IModuleInfo
{
    public ModuleInfo(string? name, Type? type)
        : this(name, type, new string[0])
    {
    }

    public ModuleInfo(string? name, Type? type, params string[] dependsOn)
    {
        if (dependsOn is null)
            throw new ArgumentNullException(nameof(dependsOn));

        ModuleName = name;
        ModuleType = type;
        ModuleTypeString = type?.AssemblyQualifiedName;
        DependsOn = new Collection<string>(dependsOn);
    }

    public ModuleInfo(Type moduleType)
          : this(moduleType, moduleType.Name) { }

    public ModuleInfo(Type moduleType, string moduleName)
           : this(moduleType, moduleName, InitializationMode.WhenAvailable) { }

    public ModuleInfo(Type moduleType, string moduleName, InitializationMode initializationMode)
           : this(moduleName, moduleType)
    {
        InitializationMode = initializationMode;
    }

    public InitializationMode InitializationMode { get; set; }
    public ModuleState State { get; set; }
    public string? ModuleName { get; set; }
    public Type? ModuleType { get; set; }
    public string? ModuleTypeString { get; set; }
    public string? Token { get; set; }
    public Collection<string> DependsOn { get;}
    public string? Ref { get; set; }
}
