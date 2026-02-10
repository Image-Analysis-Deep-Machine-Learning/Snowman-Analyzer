using Avalonia.Input;
using Snowman.DataContexts;

namespace Snowman.Controls;

public partial class FrameTimeline : UserControlWrapper<FrameTimelineDataContext>
{
    public FrameTimeline()
    {
        InitializeComponent();
        Focusable = true;
        PointerWheelChanged += (_, e) => DataContext.ProcessWheelChange(e);
        KeyDown += (_, e) => DataContext.ProcessKeyDown(e);
    }
}
