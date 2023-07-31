using PlugIn.Core.Common;

namespace PlugIn.Core.Modularity;
public class ModuleDependencySolver
{
    readonly ListDictionary<string, string> _dependencyMatrix = new ();
    readonly List<string> _knownModules = new List<string>();

    public void AddModule(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"String Cannot Be NullOrEmpty, name");

        AddToDependencyMatrix(name);
        AddToKnownModules(name);
    }

    public void AddDependency(string dependingModule, string dependentModule)
    {
        if (string.IsNullOrEmpty(dependingModule))
            throw new ArgumentException($"String Cannot Be NullOrEmpty, dependingModule");

        if (string.IsNullOrEmpty(dependentModule))
            throw new ArgumentException($"String Cannot Be NullOrEmpty, dependentModule");

        if (!_knownModules.Contains(dependingModule))
            throw new ArgumentException($"Dependency For Unknown Module, dependingModule");

        AddToDependencyMatrix(dependentModule);
        _dependencyMatrix.Add(dependentModule, dependingModule);
    }

    private void AddToDependencyMatrix(string module)
    {
        if (!_dependencyMatrix.ContainsKey(module))
            _dependencyMatrix.Add(module);
    }

    private void AddToKnownModules(string module)
    {
        if (!_knownModules.Contains(module))
            _knownModules.Add(module);
    }

    public string[] Solve()
    {
        List<string> skip = new();
        while (skip.Count < _dependencyMatrix.Count)
        {
            List<string> leaves = FindLeaves(skip);
            if (leaves.Count == 0 && skip.Count < _dependencyMatrix.Count)
                throw new Exception("CyclicDependencyFound");

            skip.AddRange(leaves);
        }
        skip.Reverse();

        if (skip.Count > _knownModules.Count)
        {
            string moduleNames = FindMissingModules(skip);
            throw new Exception($"DependencyOnMissingModule , moduleNames:{moduleNames}");
        }

        return skip.ToArray();
    }

    private string FindMissingModules(List<string> skip)
    {
        string missingModules = "";

        foreach (string module in skip)
        {
            if (!_knownModules.Contains(module))
            {
                missingModules += ", ";
                missingModules += module;
            }
        }

        return missingModules.Substring(2);
    }

    public int ModuleCount
    {
        get { return _dependencyMatrix.Count; }
    }

    private List<string> FindLeaves(List<string> skip)
    {
        List<string> result = new();

        foreach (string precedent in _dependencyMatrix.Keys)
        {
            if (skip.Contains(precedent))
                continue;

            int count = 0;
            foreach (string dependent in _dependencyMatrix[precedent])
            {
                if (skip.Contains(dependent))
                    continue;

                count++;
            }

            if (count == 0)
                result.Add(precedent);
        }
        return result;
    }
}