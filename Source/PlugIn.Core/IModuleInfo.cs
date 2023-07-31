using PlugIn.Core.Enum;

namespace PlugIn.Core;
public interface IModuleInfo : IModuleCatalogItem
{
    InitializationMode InitializationMode { get; set; }
    ModuleState State { get; set; }
    string? ModuleName { get; }
    Type? ModuleType { get; }
    string? ModuleTypeString { get; }
    string? Token { get; }
    string? Ref { get; set; }
    Collection<string> DependsOn { get; }
}
