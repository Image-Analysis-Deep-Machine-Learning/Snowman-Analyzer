using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Snowman.Controls;
using Snowman.Data;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.DataContexts;

public partial class EventTimelineViewportDataContext
{
    private readonly StackPanel _scrollViewer;
    private readonly List<TimelineOutput> _timelines = [];

    public ObservableCollection<ScriptRun> ScriptRuns { get; } = [];

    public EventTimelineViewportDataContext(IServiceProvider serviceProvider, StackPanel timelineViewer)
    {
        _scrollViewer = timelineViewer;
    }

    public void SelectedScriptRun(ScriptRun scriptRun)
    {
        foreach (var t in _timelines)
            foreach (var layer in t.Layers)
                layer.PropertyChanged -= OnLayerPropertyChanged;

        _timelines.Clear();

        foreach (var output in scriptRun.Outputs)
        {
            _timelines.Add(output);
            foreach (var layer in output.Layers)
                layer.PropertyChanged += OnLayerPropertyChanged;
        }

        UpdateTimelines();
    }


    private void OnLayerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Layer.IsVisible))
            UpdateTimelines();
    }

    private void UpdateTimelines()
    {
        _scrollViewer.Children.Clear();

        foreach (var timeline in _timelines)
        {
            _scrollViewer.Children.Add(
                new Label
                {
                    Content = timeline.Name,
                    HorizontalAlignment =  HorizontalAlignment.Right
                });
            
            _scrollViewer.Children.Add(
                new EventTimelineView(timeline)
                {
                    Background = Brushes.Transparent
                });
        }
    }
}
