using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Snowman.Controls;
using Snowman.Core.Scripting.DataSource;
using Snowman.Core.Scripting.Nodes;
using Snowman.Core.Services;
using Snowman.Data;

namespace Snowman.Designer;

public class DummyNodeService : INodeService
{
    public int ManageAndGetId(Node node) => 0;
    public void AddNodeToCanvas(Node? node) { }
    public IEnumerable<(Point StartPoint, Point EndPoint)> GetGraphConnectionTuples(bool background) => [];
    public void RegisterNodePort(NodePort nodePort) { }
    public void NodeChangedPosition(object? sender, PropertyChangedEventArgs e) { }
    public void StartConnection(Port port) { }
    public void EndConnection(PointerReleasedEventArgs e) { }
    public bool IsNewConnectionActive() => false;
    public IEnumerable<Node> GetNodes() => [];
    public void RunGraph() { }
}

public class DummyProjectService : IProjectService
{
    public Task OpenDataset(string file)
    {
        return Task.CompletedTask;
    }

    public Task OpenProject(IStorageFile file)
    {
        return Task.CompletedTask;
    }

    public Task SaveProject(IStorageFile file)
    {
        return Task.CompletedTask;
    }

    public DatasetData GetDatasetData()
    {
        return new DatasetData();
    }
}

public class DummyServiceProvider : IServiceProvider
{
    private static readonly List<IService> Services = [new DummyProjectService()];
    public T GetService<T>() where T : IService => (T)Services.FirstOrDefault(x => x.GetType().IsAssignableTo(typeof(T)))!;
    public void RegisterService<T>(T service) where T : IService { }
}
