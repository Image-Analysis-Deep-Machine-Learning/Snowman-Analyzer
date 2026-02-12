using System.Collections.Generic;
using System.ComponentModel;
using Avalonia;
using Avalonia.Input;
using Snowman.Controls;
using Snowman.Core.Scripting.DataSource;
using Snowman.Core.Scripting.Nodes;
using Snowman.Data;

namespace Snowman.Core.Services;

public interface INodeService : IService
{
    void AddNode(Node? node);
    public void RemoveNode(Node? node);
    public void RemoveConnection(Port? port1, Port? port2);
    IEnumerable<(Point StartPoint, Point EndPoint)> GetGraphConnectionTuples(bool background);
    void RegisterNodePort(NodePort nodePort);
    void NodeChangedPosition(object? sender, PropertyChangedEventArgs e);
    void StartConnection(Port port);
    void EndConnection(PointerReleasedEventArgs e);
    bool IsNewConnectionActive();
    IEnumerable<Node> GetNodes();
    void RunGraph();
    public NodeGraphData SaveGraph();
    public void LoadGraph(NodeGraphData data);
    int GetNodeIdByPort(Port port);
}
