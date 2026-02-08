using System.Collections.Generic;
using System.ComponentModel;
using Avalonia;
using Avalonia.Input;
using Snowman.Controls;
using Snowman.Core.Scripting.DataSource;
using Snowman.Core.Scripting.Nodes;
using Snowman.Core.Services;

namespace Snowman.Designer;

public class DummyNodeService : INodeService
{
    public int ManageAndGetUID(Node node) => 0;
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
