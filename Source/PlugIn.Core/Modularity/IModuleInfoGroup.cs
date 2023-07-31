using PlugIn.Core.Enum;
using System.Collections;

namespace PlugIn.Core.Modularity;
public interface IModuleInfoGroup : IModuleCatalogItem, IList<IModuleInfo>, IList
{
    InitializationMode InitializationMode { get; set; }

    string Ref { get; set; }
}