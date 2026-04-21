using System.Collections.Generic;
using Snowman.Core.Scripting.DataSource;
using Snowman.Core.Services;
using Snowman.Data;

namespace Snowman.Core.Scripting.Nodes.OutputNodes;

public class TimelineOutputNode : OutputNode
{
    private readonly ITimelineService _timelineService = null!;
    private readonly Input _input;

    private TimelineOutputNode(IServiceProvider serviceProvider) : this()
    {
        _timelineService = serviceProvider.GetService<ITimelineService>();
    }

    public TimelineOutputNode()
    {
        _input = CreateInput();
        Name = "Timeline Layer Output Node";
    }

    public override void ExecuteOutput()
    {
        base.ExecuteOutput();

        if (_input.Value is not IEnumerable<Layer> layer) return;

        var output = new TimelineOutput();

        foreach (var layerItem in layer)
        {
            output.Layers.Add(layerItem);
        }

        output.InitFilters();
        
        _timelineService.AddOutput(output);
    }

    public override string GetSystemPromptInfo()
    {
        return "Sends Layers from Input to Event Timeline control to show them on a graph (chart). " +
               $"The Type of its single Input port is {typeof(IEnumerable<Layer>)}. " +
               "It accepts multiple connections from Output ports.";
    }

    public override Node Copy(IServiceProvider serviceProvider)
    {
        var copy = new TimelineOutputNode(serviceProvider);
        return copy;
    }

    private Input CreateInput()
    {
        var input = new Input("timeline_layer", typeof(IEnumerable<Layer>), Group.Default, "Timeline Layer");
        Inputs.Add(input);
        
        return input;
    }
}
