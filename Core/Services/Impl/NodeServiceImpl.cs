using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.VisualTree;
using Snowman.Controls;
using Snowman.Core.Registries;
using Snowman.Core.Scripting;
using Snowman.Core.Scripting.DataSource;
using Snowman.Core.Scripting.Nodes;
using Snowman.Core.Scripting.Nodes.OutputNodes;
using Snowman.Core.Scripting.UserInterface;
using Snowman.Data;
using Snowman.Events.Suppliers;

namespace Snowman.Core.Services.Impl;

public class NodeServiceImpl : INodeService
{
    private readonly Canvas _viewportCanvas;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Port, NodePort> _nodePorts;
    private readonly Dictionary<Port, int> _portsToNodeIds;
    private readonly GraphOverlay _backgroundOverlay; // TODO: maybe don't send the entire overlay here, but make an event supplier for NodeService that will fire every time the node graph changes
    private readonly GraphOverlay _foregroundOverlay;
    private readonly List<OutputNode> _outputNodes;
    private ITimelineService? _timelineService;
    private readonly List<Node> _allNodes;
    private readonly PriorityQueue<int, int> _freeNodeIds;
    private readonly HashSet<int> _occupiedNodeIds;

    private Node? _selectedNode;
    private Port? _currentDragPort;
    private Point? _currentDragPoint;

    public NodeServiceImpl(Canvas viewportCanvas, GraphOverlay backgroundOverlay, GraphOverlay foregroundOverlay, IServiceProvider serviceProvider)
    {
        _nodePorts = [];
        _outputNodes = [];
        _allNodes = [];
        _portsToNodeIds = [];
        _viewportCanvas = viewportCanvas;
        _serviceProvider = serviceProvider;
        _backgroundOverlay = backgroundOverlay;
        _foregroundOverlay = foregroundOverlay;
        _freeNodeIds =  new PriorityQueue<int, int>([(0, 0)]);
        _occupiedNodeIds = [];
        _serviceProvider.GetService<IEventManager>().RegisterActionOnSupplier<INodeViewportEventSupplier>(x => x.OnPointerMovement += (_, e) => HandleNodeViewportMouseMovement(e));
    }

    public void AddNode(Node? node)
    {
        if (node is null) return;
        
        if (node.Id == -1)
        {
            node.Id = GetNextNodeId();
        }
        
        _occupiedNodeIds.Add(node.Id);
        _allNodes.Add(node);
        var builder = new NodeControlBuilder(node, _serviceProvider);
        var director = new NodeControlBuilderDirector(node, builder);
        director.Prepare();
        _viewportCanvas.Children.Add(builder.GetResult());

        if (node is OutputNode outputNode)
        {
            _outputNodes.Add(outputNode);
        }
        
        RegisterPorts(node.Inputs);
        RegisterPorts(node.Outputs);
        return;

        void RegisterPorts(IEnumerable<Port> ports)
        {
            foreach (var port in ports)
            {
                _portsToNodeIds.Add(port, node.Id);
            }
        }
    }

    public void RemoveNode(Node? node)
    {
        if (node is null) return;
        
        if (node.Id != -1)
        {
            _freeNodeIds.Enqueue(node.Id, node.Id);
        }
        
        _occupiedNodeIds.Remove(node.Id);
        _allNodes.Remove(node);
        var control = _viewportCanvas.Children.Where(x => x is NodeControl).Cast<NodeControl>().FirstOrDefault(x => x.DataContext.Node == node);
        
        if (control is null) return;
        
        _viewportCanvas.Children.Remove(control);
        
        if (node is OutputNode outputNode)
        {
            _outputNodes.Remove(outputNode);
        }
        
        foreach (var input in node.Inputs)
        {
            foreach (var connectedOutput in input.ConnectedOutputs.ToList())
            {
                RemoveConnection(connectedOutput, input);
            }
            
            _portsToNodeIds.Remove(input);
            _nodePorts.Remove(input);
        }
        
        foreach (var output in node.Outputs)
        {
            foreach (var connectedInput in output.ConnectedInputs.ToList())
            {
                RemoveConnection(connectedInput, output);
            }
            
            _portsToNodeIds.Remove(output);
            _nodePorts.Remove(output);
        }
    }

    public void RemoveConnection(Port? port1, Port? port2)
    {
        var input = port1 is Input ? port1 as Input : port2 is Input ? port2 as Input : null;
        var output = port1 is Output ? port1 as Output : port2 is Output ? port2 as Output : null;
        
        if (input is null || output is null) return;
        
        input.ConnectedOutputs.Remove(output);
        output.ConnectedInputs.Remove(input);
        InvalidateBackgroundOverlay();
    }
    
    public IEnumerable<(Point StartPoint, Point EndPoint)> GetGraphConnectionTuples(bool background)
    {
        var retList = new List<(Point StartPoint, Point EndPoint)>();
        
        if (background)
        {
            foreach (var nodePort in _nodePorts)
            {
                var connectedOutputs = (nodePort.Key as Input)?.ConnectedOutputs;
            
                if (connectedOutputs is null) continue;

                Matrix? inputTransform;
                Point inputPoint;
                var inputExpander = FindFirstRetractedExpander(nodePort.Value);

                if (inputExpander is not null)
                {
                    inputTransform = inputExpander.TransformToVisual(_viewportCanvas);
                    inputPoint = new Point(0, inputExpander.Bounds.Height / 2);
                }

                else
                {
                    inputTransform = nodePort.Value.TransformToVisual(_viewportCanvas);
                    inputPoint = new Point(nodePort.Value.Bounds.Width / 2, nodePort.Value.Bounds.Height / 2);
                }
                
                if (inputTransform is null) continue;
                
                inputPoint = inputPoint.Transform(inputTransform.Value);

                foreach (var outputNode in connectedOutputs.Select(output => _nodePorts[output]))
                {
                    Matrix? outputTransform;
                    Point outputPoint;
                    var outputExpander = FindFirstRetractedExpander(outputNode);
                    
                    if (outputExpander is not null)
                    {
                        outputTransform = outputExpander.TransformToVisual(_viewportCanvas);
                        var headerControl = outputExpander.FindDescendantOfType<LayoutTransformControl>()!;
                        
                        outputPoint = new Point(headerControl.Bounds.Width, headerControl.Bounds.Height / 2);
                    }

                    else
                    {
                        outputTransform = outputNode.TransformToVisual(_viewportCanvas);
                        outputPoint = new Point(outputNode.Bounds.Width / 2, outputNode.Bounds.Height / 2);
                    }
                
                    if (outputTransform is null) continue;
                
                    outputPoint = outputPoint.Transform(outputTransform.Value);
                    retList.Add((inputPoint, outputPoint));
                }
            }
        }
        
        else if (IsNewConnectionActive())
        {
            var nodePort = _nodePorts[_currentDragPort!];
            var transform = nodePort.TransformToVisual(_viewportCanvas);

            if (transform is null) return retList;
            
            var nodePortPoint = new Point(nodePort.Bounds.Width / 2, nodePort.Bounds.Height / 2).Transform(transform.Value);
            retList.Add((nodePortPoint, _currentDragPoint!.Value));
        }
        
        return retList;
    }

    public void RegisterNodePort(NodePort nodePort)
    {
        _nodePorts.Add(nodePort.Port, nodePort);
    }

    public void NodeChangedPosition(object? sender, PropertyChangedEventArgs e)
    {
        InvalidateBackgroundOverlay();
    }

    public void StartConnection(Port port)
    {
        _currentDragPort = port;
        
        var transform = _nodePorts[port].TransformToVisual(_viewportCanvas);
        
        if (transform is null) return;
        
        _currentDragPoint = new Point(_nodePorts[port].Bounds.Width / 2, _nodePorts[port].Bounds.Height / 2).Transform(transform.Value);
        InvalidateForegroundOverlay();
    }

    public void EndConnection(PointerReleasedEventArgs e)
    {
        var hit = _viewportCanvas.InputHitTest(e.GetPosition(_viewportCanvas), x => x is not GraphOverlay, false);
        var ancestor = (hit as Ellipse).FindAncestorOfType<NodePort>();
        
        if (ancestor is not null)
            ConnectPorts(_currentDragPort, ancestor.Port);
        
        _currentDragPort = null;
        _currentDragPoint = null;
        InvalidateForegroundOverlay();
    }

    public bool IsNewConnectionActive()
    {
        return _currentDragPort is not null && _currentDragPoint is not null;
    }

    public IEnumerable<Node> GetNodes()
    {
        return ScriptNodeRegistry.GetPrototypeCopies().Cast<Node>().Concat(OutputNodeRegistry.GetPrototypeCopies());
    }
    
    public void ExecuteGraph()
    {
        GetTimelineService().StartNewScriptRun();
        var error = false;

        try
        {
            foreach (var outputNode in _outputNodes)
            {
                outputNode.ExecuteOutput();
            }
        }

        catch (Exception)
        {
            error = true;
            throw;
        }

        finally
        {
            foreach (var outputNode in _outputNodes)
            {
                outputNode.Reset(error);
            }
        }
    }

    public NodeGraphData SaveGraph()
    {
        var graphData = new NodeGraphData();

        foreach (var node in _allNodes)
        {
            graphData.Nodes.Add(node.Serialize(_serviceProvider));
        }

        return graphData;
    }

    public void LoadGraph(NodeGraphData data)
    {
        foreach (var node in _allNodes.ToList())
        {
            RemoveNode(node);
        }
        
        var inputDict = new Dictionary<InputData, Input>();

        foreach (var nodeData in data.Nodes)
        {
            var copy = nodeData.Type switch
            {
                nameof(ScriptNode) => ScriptNodeRegistry.GetCopy(nodeData.UniqueIdentifier, _serviceProvider),
                nameof(OutputNode) =>
                    OutputNodeRegistry.GetCopy(nodeData.UniqueIdentifier, _serviceProvider),
                _ => throw new Exception($"{nodeData.Type} with unique ID {nodeData.UniqueIdentifier} is not registered.")
            };
            
            copy.Deserialize(nodeData, _serviceProvider);
            AddNode(copy);
            
            foreach (var inputData in nodeData.Inputs)
            {
                var input = copy.Inputs.First(input => input.Name == inputData.Name);
                
                inputDict[inputData] = input;
            }
        }

        // the entire graph must exist before adding connections
        foreach (var nodeData in data.Nodes)
        {
            foreach (var inputData in nodeData.Inputs)
            {
                var input = inputDict[inputData];
                
                foreach (var connectedOutputData in inputData.ConnectedOutputs)
                {
                    var connectedOutputNode = _allNodes.First(x => x.Id == connectedOutputData.NodeId);
                    var output = connectedOutputNode.Outputs.First(output => output.Name == connectedOutputData.OutputName);
                    ConnectPorts(input, output);
                }
            }
        }
    }

    public int GetNodeIdByPort(Port port)
    {
        return _portsToNodeIds[port];
    }

    public void SelectNode(Node node)
    {
        _selectedNode = node;
    }

    public void RemoveSelectedNode()
    {
        if (_selectedNode is null) return;
        
        RemoveNode(_selectedNode);
        _selectedNode = null;
    }

    private static Expander? FindFirstRetractedExpander(NodePort outputNode)
    {
        var currentExpander = outputNode.FindAncestorOfType<Expander>();

        if (currentExpander?.IsExpanded ?? true) return null;

        while (!currentExpander.FindAncestorOfType<Expander>()?.IsExpanded ?? false)
        {
            currentExpander = currentExpander.FindAncestorOfType<Expander>();
        }
        
        return currentExpander;
    }

    private (bool CanConnect, Input? Input, Output? Output) CanConnectPorts(Port? port1, Port? port2)
    {
        var input = port1 is Input ? port1 as Input : port2 is Input ? port2 as Input : null;
        var output = port1 is Output ? port1 as Output : port2 is Output ? port2 as Output : null;

        return (CanConnect:
            input is not null &&
            output is not null &&
            IsDifferentNode(input, output) &&
            ArePortsTypeCompatible(input, output), input, output);
    }

    private static bool ArePortsTypeCompatible(Input input, Output output)
    {
        if (!input.MultipleConnectionsAllowed) return input.Type.IsAssignableFrom(output.Type) && input.ConnectedOutputs.Count == 0;
        
        var genericArgument = input.Type.GetGenericArguments().FirstOrDefault() ?? typeof(object);
        return genericArgument.IsAssignableFrom(output.Type);

    }

    private bool IsDifferentNode(Port port1, Port port2)
    {
        var id1 = _portsToNodeIds[port1];
        var id2 = _portsToNodeIds[port2];
        
        return id1 != id2;
    }

    private void ConnectPorts(Port? port1, Port? port2)
    {
        var testResult = CanConnectPorts(port1, port2);

        if (!testResult.CanConnect) return;
        
        testResult.Input?.ConnectedOutputs.Add(testResult.Output!);
        testResult.Output?.ConnectedInputs.Add(testResult.Input!);
        InvalidateBackgroundOverlay();
    }

    private void HandleNodeViewportMouseMovement(PointerEventArgs e)
    {
        if (!IsNewConnectionActive()) return;
        
        _currentDragPoint = e.GetPosition(_viewportCanvas);
        
        var hit = _viewportCanvas.InputHitTest(e.GetPosition(_viewportCanvas), x => x is not GraphOverlay, false);
        var ancestor = (hit as Ellipse).FindAncestorOfType<NodePort>();

        if (ancestor is not null)
        {
            var transform = ancestor.TransformToVisual(_viewportCanvas);

            if (transform is not null && CanConnectPorts(_currentDragPort, ancestor.Port).CanConnect)
            {
                _currentDragPoint = new Point(ancestor.Bounds.Width / 2, ancestor.Bounds.Height / 2).Transform(transform.Value);
            }
        }
        
        InvalidateForegroundOverlay();
    }

    private void InvalidateBackgroundOverlay()
    {
        _backgroundOverlay.InvalidateVisual();
    }
    
    private void InvalidateForegroundOverlay()
    {
        _foregroundOverlay.InvalidateVisual();
    }
    
    private int GetNextNodeId()
    {
        int nextId;
        
        do
        {
            nextId = _freeNodeIds.Dequeue();
            
            if (_freeNodeIds.Count == 0)
            {
                _freeNodeIds.Enqueue(nextId + 1, nextId + 1);
            }
        } while (_occupiedNodeIds.Contains(nextId));
        
        return nextId;
    }

    private ITimelineService GetTimelineService()
    {
        _timelineService ??= _serviceProvider.GetService<ITimelineService>();

        return _timelineService;
    }
}
