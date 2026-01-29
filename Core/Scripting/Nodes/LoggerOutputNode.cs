using Snowman.Core.Scripting.DataSource;
using Snowman.Core.Services;

namespace Snowman.Core.Scripting.Nodes;

public class LoggerOutputNode : OutputNode
{
    private ILoggerService _loggerService = null!;
    private readonly Input _messageInput;
    
    public LoggerOutputNode()
    {
        _messageInput = CreateInput();
    }

    public override Node Copy(IServiceProvider serviceProvider)
    {
        var copy = new LoggerOutputNode();
        copy._loggerService = serviceProvider.GetService<ILoggerService>();
        return copy;
    }

    public override void ExecuteOutput()
    {
        base.ExecuteOutput();
        var message = _messageInput.Value as string;
        _loggerService.LogMessage(message);
    }

    private Input CreateInput()
    {
        var stringInput = new Input("logger_input", typeof(string), Group.Default, "Message");
        Inputs.Add(stringInput);
        
        return stringInput;
    }
}
