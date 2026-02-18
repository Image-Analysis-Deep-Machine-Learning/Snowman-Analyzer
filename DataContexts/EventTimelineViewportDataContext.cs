using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Snowman.Controls;
using Snowman.Data;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.DataContexts;

public partial class EventTimelineViewportDataContext
{
    private const int EventTimelineHeight = 100 + EventTimelineView.XAxisHeight;
    private readonly StackPanel _timelineViewer;
    private readonly List<TimelineOutput> _timelines = [];

    public ObservableCollection<ScriptRun> ScriptRuns { get; } = [];

    public EventTimelineViewportDataContext(IServiceProvider serviceProvider, StackPanel timelineViewer)
    {
        _timelineViewer = timelineViewer;
    }

    public void SelectedScriptRun(ScriptRun scriptRun)
    {
        _timelines.Clear();

        foreach (var output in scriptRun.Outputs)
            _timelines.Add(output);

        UpdateTimelines();
    }

    private void UpdateTimelines()
    {
        _timelineViewer.Children.Clear();

        foreach (var timeline in _timelines)
        {
            _timelineViewer.Children.Add(
                new Label
                {
                    Content = timeline.Name,
                    HorizontalAlignment =  HorizontalAlignment.Right,
                    Margin = new Thickness(0, 0, 10, 0)
                });
            
            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("20, *"),
                Height = EventTimelineHeight,
                Margin = new Thickness(0, 0, 10, 0)
            };
            
            var timelinePanel = new EventTimelineView(timeline)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            
            grid.Children.Add(timelinePanel);
            Grid.SetColumn(timelinePanel, 1);
            
            var yAxisPanel = new YAxisPanel
            {
                MaxY = timeline.MaxY,
                MinY = timeline.MinY,
                Height = EventTimelineHeight - EventTimelineView.XAxisHeight,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 0, 5, 0)
            };
            
            grid.Children.Add(yAxisPanel);
            Grid.SetColumn(yAxisPanel, 0); 

            yAxisPanel.InvalidateVisual();
            
            _timelineViewer.Children.Add(grid);
        }
    }
}
