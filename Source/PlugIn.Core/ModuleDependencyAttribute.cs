namespace PlugIn.Core;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class ModuleDependencyAttribute : Attribute
{
    public ModuleDependencyAttribute(string moduleName)
    {
        ModuleName = moduleName;
    }

    public string ModuleName { get; }
}