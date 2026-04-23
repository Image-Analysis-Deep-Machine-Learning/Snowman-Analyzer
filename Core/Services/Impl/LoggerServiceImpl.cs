using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Threading;

namespace Snowman.Core.Services.Impl;

public class LoggerServiceImpl : ILoggerService
{
    private readonly TextBox _outputLogTextBox;

    public LoggerServiceImpl(TextBox outputLogTextBox)
    {
        _outputLogTextBox = outputLogTextBox;
    }
    
    public void LogMessage(string? message)
    {
        Debug.Print(message);
        
        Dispatcher.UIThread.Post(() =>
        {
            _outputLogTextBox.Text += $"{message}\n";
            _outputLogTextBox.CaretIndex = _outputLogTextBox.Text.Length;
            _outputLogTextBox.ScrollToLine(_outputLogTextBox.GetLineCount() - 1);
        });
    }
}
