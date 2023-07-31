﻿using System.Reflection;

namespace PlugIn.Core.Modularity;
public class AssemblyResolver : IAssemblyResolver, IDisposable
{
    private readonly List<AssemblyInfo> registeredAssemblies = new();

    private bool handlesAssemblyResolve;

    public void LoadAssemblyFrom(string assemblyFilePath)
    {
        if (!handlesAssemblyResolve)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            handlesAssemblyResolve = true;
        }

        var assemblyUri = GetFileUri(assemblyFilePath);
        if (assemblyUri == null)
            throw new ArgumentException("InvalidArgumentAssemblyUri", nameof(assemblyFilePath));

        if (!File.Exists(assemblyUri.LocalPath))
            throw new FileNotFoundException(null, assemblyUri.LocalPath);

        AssemblyName assemblyName = AssemblyName.GetAssemblyName(assemblyUri.LocalPath);
        AssemblyInfo assemblyInfo = registeredAssemblies.FirstOrDefault(a => assemblyName == a.AssemblyName);

        if (assemblyInfo != null)
            return;

        assemblyInfo = new AssemblyInfo() { AssemblyName = assemblyName, AssemblyUri = assemblyUri };
        registeredAssemblies.Add(assemblyInfo);
    }

    private static Uri? GetFileUri(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return null;

        Uri uri;
        if (!Uri.TryCreate(filePath, UriKind.Absolute, out uri))
            return null;

        if (!uri.IsFile)
            return null;

        return uri;
    }

    private Assembly? CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
        AssemblyName assemblyName = new(args.Name);
        AssemblyInfo assemblyInfo = registeredAssemblies.FirstOrDefault(a => AssemblyName.ReferenceMatchesDefinition(assemblyName, a.AssemblyName));

        if (assemblyInfo != null)
        {
            if (assemblyInfo.Assembly == null)
                assemblyInfo.Assembly = Assembly.LoadFrom(assemblyInfo.AssemblyUri.LocalPath);

            return assemblyInfo.Assembly;
        }

        return default;
    }

    private class AssemblyInfo
    {
        public required AssemblyName AssemblyName { get; set; }

        public required Uri AssemblyUri { get; set; }

        public Assembly? Assembly { get; set; }
    }

    #region Implementation of IDisposable

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <remarks>Calls <see cref="Dispose(bool)"/></remarks>.
    /// <filterpriority>2</filterpriority>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the associated <see cref="AssemblyResolver"/>.
    /// </summary>
    /// <param name="disposing">When <see langword="true"/>, it is being called from the Dispose method.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (handlesAssemblyResolve)
        {
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
            handlesAssemblyResolve = false;
        }
    }

    #endregion
}