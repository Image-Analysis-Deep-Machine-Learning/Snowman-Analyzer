using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.VisualTree;
using Snowman.Controls;
using Snowman.Core.Scripting;
using Snowman.Core.Scripting.DataSource;
using Snowman.Core.Scripting.Nodes;
using Snowman.Core.Scripting.Nodes.OutputNodes;
using Snowman.Core.Scripting.UserInterface;
using Snowman.Events.Suppliers;

namespace Snowman.Core.Services.Impl;

public class NodeServiceImpl : INodeService
{
    private const string ScriptFileExtension = ".script";
    private const string ScriptsFolder = "Scripts";

    private readonly List<Node> ScriptPrototypes;
    private readonly List<Node> OutputNodePrototypes;
    private readonly Canvas _viewportCanvas;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Port, NodePort>  _nodePorts;
    private readonly GraphOverlay _backgroundOverlay; // TODO: maybe don't send the entire overlay here, but make an event supplier for NodeService that will fire every time the node graph changes
    private readonly GraphOverlay _foregroundOverlay;
    private readonly List<OutputNode> _outputNodes;
    
    private Port? _currentDragPort;
    private Point? _currentDragPoint;

    public NodeServiceImpl(Canvas viewportCanvas, GraphOverlay backgroundOverlay, GraphOverlay foregroundOverlay, IServiceProvider serviceProvider)
    {
        ScriptPrototypes = [];
        OutputNodePrototypes = [];
        _nodePorts = [];
        _outputNodes = [];
        _viewportCanvas = viewportCanvas;
        _serviceProvider = serviceProvider;
        _backgroundOverlay = backgroundOverlay;
        _foregroundOverlay = foregroundOverlay;
        _serviceProvider.GetService<IEventManager>().RegisterActionOnSupplier<INodeViewportEventSupplier>(x => x.OnPointerMovement += (_, e) => HandleNodeViewportMouseMovement(e));
        LoadScripts();
        LoadOutputNodes();
    }

    public int ManageAndGetId(Node node)
    {
        return 0; // TODO: do something about this
    }

    public void AddNodeToCanvas(Node? node)
    {
        if (node is null) return;
        
        var builder = new NodeControlBuilder(node, _serviceProvider);
        var director = new NodeControlBuilderDirector(node, builder);
        director.Prepare();
        _viewportCanvas.Children.Add(builder.GetResult());

        if (node is OutputNode outputNode)
        {
            _outputNodes.Add(outputNode);
        }
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
        var hit = _viewportCanvas.InputHitTest(e.GetPosition(_viewportCanvas), x =>
        {
            return x is not GraphOverlay;
        }, false);
        
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
        return ScriptPrototypes.Concat(OutputNodePrototypes).ToImmutableList();
    }
    
    public void RunGraph()
    {
        var graphTask = new Task(() =>
        {
            foreach (var outputNode in _outputNodes)
            {
                outputNode.ExecuteOutput();
            }
            
            foreach (var outputNode in _outputNodes)
            {
                outputNode.Reset();
            }
        });
        
        graphTask.Start();
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
        if (!input.MultipleConnectionsAllowed) return input.Type.IsAssignableFrom(output.Type);
        
        var genericArgument = input.Type.GetGenericArguments().FirstOrDefault() ?? typeof(object);
        return genericArgument.IsAssignableFrom(output.Type);

    }

    private bool IsDifferentNode(Port port1, Port port2)
    {
        var nodePort1 = _nodePorts[port1];
        var nodePort2 = _nodePorts[port2];
        var nodeControl1 = nodePort1.FindAncestorOfType<NodeControl>();
        var nodeControl2 = nodePort2.FindAncestorOfType<NodeControl>();

        return nodeControl1 != nodeControl2;
    }

    private void ConnectPorts(Port? port1, Port? port2)
    {
        var testResult = CanConnectPorts(port1, port2);

        if (!testResult.CanConnect) return;
        
        testResult.Input?.ConnectedOutputs.Add(testResult.Output!);
        InvalidateBackgroundOverlay();
    }

    private void HandleNodeViewportMouseMovement(PointerEventArgs e)
    {
        if (!IsNewConnectionActive()) return;
        
        _currentDragPoint = e.GetPosition(_viewportCanvas);
        
        var hit = _viewportCanvas.InputHitTest(e.GetPosition(_viewportCanvas), x =>
        {
            return x is not GraphOverlay;
        }, false);
        
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
    

    private void LoadOutputNodes()
    {
        OutputNodePrototypes.Add(new LoggerOutputNode());
    }

    private void LoadScripts()
    {
        var dirInfo = new DirectoryInfo(ScriptsFolder);

        foreach (var fileInfo in dirInfo.GetFiles())
        {
            if (fileInfo.Extension == ScriptFileExtension)
            {
                var script = Script.Load(fileInfo.FullName);
                var scriptNode = ScriptParser.Parse(script, _serviceProvider);
                ScriptPrototypes.Add(scriptNode);
            }
        }
    }
}
