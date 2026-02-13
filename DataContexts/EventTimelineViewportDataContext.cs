using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Media;
using Snowman.Controls;
using Snowman.Data;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.DataContexts;

public partial class EventTimelineViewportDataContext
{
    private readonly StackPanel _scrollViewer;
    private List<TimelineOutput> _timelines = [];

    public ObservableCollection<ScriptRun> ScriptRuns { get; } = [];

    public double Zoom { get; private set; } = 1.0;
    public double Pan { get; private set; } = 0.0;

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
            _scrollViewer.Children.Add(new EventTimeline(timeline) { Background = Brushes.Transparent});
        }
    }

    // public void AddTimeline(EventTimelineDataContext timeline)
    // {
    //     timeline.AttachToViewport(this);
    //     Timelines.Add(timeline);
    // }
    //
    // public void ApplyZoom(double delta)
    // {
    //     Zoom = Math.Clamp(Zoom + delta, 0.1, 10);
    //     foreach (var t in Timelines)
    //         t.OnZoomChanged(Zoom);
    // }
    //
    // public void ApplyPan(double delta)
    // {
    //     Pan += delta;
    //     foreach (var t in Timelines)
    //         t.OnPanChanged(Pan);
    // }
}
