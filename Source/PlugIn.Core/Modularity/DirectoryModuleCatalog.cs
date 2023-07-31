using PlugIn.Core.Enum;
using PlugIn.Core.Extensions;
using System.Reflection;
using System.Reflection.Emit;

namespace PlugIn.Core.Modularity;

public class DirectoryModuleCatalog : ModuleCatalog
{
    /// <summary>
    /// Directory containing modules to search for.
    /// </summary>
    public string? ModulePath { get; set; }

    /// <summary>
    /// Drives the main logic of building the child domain and searching for the assemblies.
    /// </summary>
    protected override void InnerLoad()
    {
        if (string.IsNullOrEmpty(ModulePath))
            throw new InvalidOperationException($"ModulePath Cannot Be NullOrEmpty");

        if (!Directory.Exists(ModulePath))
            Directory.CreateDirectory(ModulePath);

        AppDomain childDomain = AppDomain.CurrentDomain;

        try
        {
            List<string> loadedAssemblies = new();

            var assemblies = (
                                 from Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()
                                 where !(assembly is AssemblyBuilder)
                                    && assembly.GetType().FullName != "System.Reflection.Emit.InternalAssemblyBuilder"
                                    && !string.IsNullOrEmpty(assembly.Location)
                                 select assembly.Location
                             );

            loadedAssemblies.AddRange(assemblies);

            Type loaderType = typeof(InnerModuleInfoLoader);

            if (loaderType.Assembly != null)
            {
                var loaderObject = ActivatorExtensions.CreateInstanceFrom(loaderType.Assembly.Location, loaderType.FullName);
                var loader = loaderObject as InnerModuleInfoLoader;
                if (loader is null) return;

                Items.AddRange(loader.GetModuleInfos(ModulePath!));
            }
        }
        catch (Exception ex)
        {
            throw new Exception("There was an error loading assemblies.", ex);
        }
    }

    private class InnerModuleInfoLoader : MarshalByRefObject
    {
        internal IModuleInfo[] GetModuleInfos(string path)
        {
            DirectoryInfo directory = new(path);
            ResolveEventHandler resolveEventHandler = (sender, args) => OnReflectionOnlyResolve(args, directory);

            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += resolveEventHandler;

            Assembly moduleReflectionOnlyAssembly = AppDomain.CurrentDomain.GetAssemblies().First(asm => asm.FullName == typeof(IModule).Assembly.FullName);
            Type IModuleType = moduleReflectionOnlyAssembly.GetType(typeof(IModule).FullName);

            List<ModuleInfo> moduleInfos = new();
            var modules = GetNotAlreadyLoadedModuleInfos(directory, IModuleType);
            moduleInfos.AddRange(modules);

            var subDirectories = directory.GetDirectories();
            if (subDirectories is not null)
            {
                foreach (var subDirectory in subDirectories)
                {
                    modules = GetNotAlreadyLoadedModuleInfos(subDirectory, IModuleType);
                    moduleInfos.AddRange(modules);
                }
            }
            var array = moduleInfos.ToArray();
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= resolveEventHandler;
            return array;
        }

        private static IEnumerable<ModuleInfo> GetNotAlreadyLoadedModuleInfos(DirectoryInfo directory, Type IModuleType)
        {
            List<Assembly> validAssemblies = new();
            Assembly[] alreadyLoadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(p => !p.IsDynamic).ToArray();

            var fileInfos = directory.GetFiles("*.dll")
                                     .Where(file => alreadyLoadedAssemblies.FirstOrDefault(assembly => string.Compare(Path.GetFileName(assembly.Location), file.Name, StringComparison.OrdinalIgnoreCase) == 0) == null)
                                     .ToList();

            foreach (FileInfo fileInfo in fileInfos)
            {
                try
                {
                    validAssemblies.Add(Assembly.LoadFrom(fileInfo.FullName));
                }
                catch (BadImageFormatException)
                {
                    // skip non-.NET Dlls
                }
            }

            return validAssemblies.SelectMany(assembly => assembly.GetExportedTypes()
                                                                  .Where(IModuleType.IsAssignableFrom)
                                                                  .Where(t => t != IModuleType)
                                                                  .Where(t => !t.IsAbstract)
                                                                  .Select(type => CreateModuleInfo(type)));
        }

        private static Assembly OnReflectionOnlyResolve(ResolveEventArgs args, DirectoryInfo directory)
        {
            Assembly loadedAssembly = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies().FirstOrDefault(
                asm => string.Equals(asm.FullName, args.Name, StringComparison.OrdinalIgnoreCase));
            if (loadedAssembly != null)
                return loadedAssembly;

            AssemblyName assemblyName = new AssemblyName(args.Name);
            string dependentAssemblyFilename = Path.Combine(directory.FullName, assemblyName.Name + ".dll");
            if (File.Exists(dependentAssemblyFilename))
                return Assembly.ReflectionOnlyLoadFrom(dependentAssemblyFilename);

            return Assembly.ReflectionOnlyLoad(args.Name);
        }

        internal void LoadAssemblies(IEnumerable<string> assemblies)
        {
            foreach (string assemblyPath in assemblies)
            {
                try
                {
                    Assembly.ReflectionOnlyLoadFrom(assemblyPath);
                }
                catch (FileNotFoundException)
                {
                    // Continue loading assemblies even if an assembly can not be loaded in the new AppDomain
                }
            }
        }

        private static ModuleInfo CreateModuleInfo(Type type)
        {
            string moduleName = type.Module.Name;
            string? token = default;
            List<string> dependsOn = new();
            bool onDemand = false;

            var moduleAttribute = type.GetCustomAttribute<ModuleAttribute>();
            if (moduleAttribute != null)
            {
                token = moduleAttribute.Token;
                onDemand = moduleAttribute.OnDemand;
            }

            var moduleDependencyAttributes = type.GetCustomAttributes<ModuleDependencyAttribute>();
            if (moduleDependencyAttributes is not null)
            {
                foreach (var dependencyAttribute in moduleDependencyAttributes)
                    dependsOn.Add(dependencyAttribute.ModuleName);
            }

            ModuleInfo moduleInfo = new(moduleName, type)
            {
                Token = token,
                InitializationMode = onDemand ? InitializationMode.OnDemand : InitializationMode.WhenAvailable,
                Ref = type.Assembly.EscapedCodeBase,
            };
            moduleInfo.DependsOn.AddRange(dependsOn);
            return moduleInfo;
        }
    }
}
