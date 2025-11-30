using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Snowman.Core.Services;
using Snowman.Core.Tools;
using Snowman.DataContexts;
using Snowman.Events.Viewport;

namespace Snowman.Controls;

public partial class ToolBar : ServiceableUserControl<ToolBarDataContext>
{
    public ToolBar()
    {
        InitializeComponent();
    }
}
