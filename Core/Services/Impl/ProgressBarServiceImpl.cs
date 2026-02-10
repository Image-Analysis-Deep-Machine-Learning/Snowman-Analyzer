using System.Threading.Tasks;
using Avalonia.Controls;
using Dispatcher = Avalonia.Threading.Dispatcher;

namespace Snowman.Core.Services.Impl;

public class ProgressBarServiceImpl : IProgressBarService
{
    private readonly ProgressBar _progressBarControl;
    private readonly TextBlock _messageTextBox;

    public ProgressBarServiceImpl(ProgressBar progressBarControl, TextBlock messageTextBox)
    {
        _progressBarControl = progressBarControl;
        _messageTextBox = messageTextBox;
    }

    public void StartProgress(string jobDescription)
    {
        Dispatcher.UIThread.Post(() =>
        {
            _progressBarControl.Value = 0;
            _progressBarControl.IsVisible = true;
            _messageTextBox.Text = jobDescription;
            _messageTextBox.IsVisible = true;
        });
    }

    public void SetProgress(int percentage)
    {
        Dispatcher.UIThread.Post(() =>
        {
            _progressBarControl.Value = percentage;
        });
    }

    public void FinishProgress(string finishMessage)
    {
        Dispatcher.UIThread.Post(() =>
        {
            _messageTextBox.Text = finishMessage;
            
            Task.Delay(3000).ContinueWith(_ =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    _progressBarControl.Value = 0;
                    _progressBarControl.IsVisible = false;
                    _messageTextBox.IsVisible = false;
                });
            });
        });
    }
}
