using System.Collections.Generic;
using Snowman.Core.Scripting.DataSource;
using Snowman.Core.Services;

namespace Snowman.Core.Scripting.Nodes.OutputNodes;

public class LoggerOutputNode : OutputNode
{
    private readonly ILoggerService _loggerService = null!;
    private readonly Input _messageInput;

    private LoggerOutputNode(IServiceProvider serviceProvider) : this()
    {
        _loggerService = serviceProvider.GetService<ILoggerService>();
    }
    
    public LoggerOutputNode()
    {
        _messageInput = CreateInput();
        Name = "Logger Output";
        UniqueIdentifier = nameof(LoggerOutputNode);
    }

    public override Node Copy(IServiceProvider serviceProvider)
    {
        var copy = new LoggerOutputNode(serviceProvider);
        return copy;
    }

    public override void ExecuteOutput()
    {
        base.ExecuteOutput(); // TODO: maybe change these overrides to template methods so I don't need to call base.XXX() every time?
        var messages = _messageInput.Value as IEnumerable<object?>;

        foreach (var message in messages ?? [])
        {
            _loggerService.LogMessage(message?.ToString());
        }
        
        IsReady = true;
    }

    public override string GetSystemPromptInfo()
    {
        return "Prints string representation of incoming data into a Console Output textbox in Main Window. " + 
               $"The Type of its single Input port is {typeof(IEnumerable<object?>)}. " +
               "It accepts multiple connections from Output ports of any Type.";
    }

    private Input CreateInput()
    {
        var stringInput = new Input("logger_input", typeof(IEnumerable<object?>), Group.Default, "Message");
        Inputs.Add(stringInput);
        
        return stringInput;
    }
}
