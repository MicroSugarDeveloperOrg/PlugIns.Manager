namespace PlugIn.Core;

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module, AllowMultiple = false)]
public class ModularAttribute : Attribute
{
    public ModularAttribute(Type type)
    {
        Type = type;
        ModuleName = type.Module.Name;
    }
    public bool OnDemand { get; set; } = false;
    public string? Token { get; set; }
    public Type Type { get; }
    public string ModuleName { get; set; }
}


[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module, AllowMultiple = false)]
public class ModularAttribute<T> : ModularAttribute where T : IModule
{
    public ModularAttribute()
        : base(typeof(T))
    {
 
    }
}
