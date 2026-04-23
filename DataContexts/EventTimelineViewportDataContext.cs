using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Snowman.Controls;
using Snowman.Data;

namespace Snowman.DataContexts;

public partial class EventTimelineViewportDataContext
{
    private readonly StackPanel _timelineViewer;
    private readonly List<TimelineOutput> _timelines = [];

    public ObservableCollection<ScriptRun> ScriptRuns { get; } = [];

    public EventTimelineViewportDataContext(StackPanel timelineViewer)
    {
        _timelineViewer = timelineViewer;
    }

    public void SelectedScriptRun(ScriptRun scriptRun)
    {
        _timelines.Clear();
        _timelines.AddRange(scriptRun.Outputs);
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
                    Margin = new Thickness(0, 0, 10, 0),
                    Foreground = Brushes.Gray
                });
            
            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("20, *"),
                Height = EventTimelineView.EventTimelineHeight,
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
                Height = EventTimelineView.EventTimelineHeight - EventTimelineView.XAxisHeight + EventTimelineView.TopPadding,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, -1, 5, 0)
            };
            
            grid.Children.Add(yAxisPanel);
            Grid.SetColumn(yAxisPanel, 0); 

            yAxisPanel.InvalidateVisual();
            
            _timelineViewer.Children.Add(grid);
        }
    }
}
