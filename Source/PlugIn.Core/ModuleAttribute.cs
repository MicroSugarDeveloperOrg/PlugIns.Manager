namespace PlugIn.Core;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ModuleAttribute: Attribute
{
    public ModuleAttribute(Type type)
    {
        Type = type;
        ModuleName = type.Module.Name;
    }

    public bool OnDemand { get; set; } = false;
    public string ModuleName { get; set; }
    public string? Token { get; set; }
    public Type Type { get; }
}


[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ModuleAttribute<T> : ModuleAttribute where T : IModule
{
    public ModuleAttribute()
      : base(typeof(T))
    {

    }

}