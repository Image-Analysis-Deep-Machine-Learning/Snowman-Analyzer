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
    public void AddNode(Node? node) { }
    public void RemoveNode(Node? node) { }
    public void RemoveConnection(Port? port1, Port? port2) { }
    public IEnumerable<(Point StartPoint, Point EndPoint)> GetGraphConnectionTuples(bool background) => [];
    public void RegisterNodePort(NodePort nodePort) { }
    public void NodeChangedPosition(object? sender, PropertyChangedEventArgs e) { }
    public void StartConnection(Port port) { }
    public void EndConnection(PointerReleasedEventArgs e) { }
    public bool IsNewConnectionActive() => false;
    public IEnumerable<Node> GetNodes() => [];
    public void RunGraph() { }
    public NodeGraphData SaveGraph() => new();
    public void LoadGraph(NodeGraphData data) { }
    public int GetNodeIdByPort(Port port) => 0;
}

public class DummyProjectService : IProjectService
{
    public Task OpenDataset(string file) => Task.CompletedTask;
    public Task OpenProject(IStorageFile file) => Task.CompletedTask;
    public Task SaveProject(IStorageFile file) => Task.CompletedTask;
    public DatasetData GetDatasetData() => new();
    public void HighlightByTrackId(int trackId) { }
    public void ClearHighlights() { }
}

public class DummyServiceProvider : IServiceProvider
{
    private static readonly List<IService> Services = [new DummyProjectService()];
    public static readonly DummyServiceProvider Instance = new();
    public T GetService<T>() where T : IService => (T)Services.FirstOrDefault(x => x.GetType().IsAssignableTo(typeof(T)))!;
    public void RegisterService<T>(T service) where T : IService { }
}
