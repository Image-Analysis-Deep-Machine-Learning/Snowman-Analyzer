using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Media;
using Snowman.DataContexts;
using Snowman.Utilities;

namespace Snowman.Data;

public class TimelineOutput : INotifyPropertyChanged
{
    private static int _outputCounter;
    public string Name { get; set; } = "Output " + (_outputCounter++ + 1);
    public List<Layer> Layers { get; set; } = [];
    
    public double MaxY
    {
        get
        {
            var maxY = 0.0;

            foreach (var layer in Layers) 
            {
                var layerMax = layer.MaxY;
                if (layerMax > maxY) maxY = layerMax;
            }

            return Math.Max(maxY, 1);
        }
    }
    
    public double MinY
    {
        get
        {
            var minY = double.MaxValue;

            foreach (var layer in Layers)
            {
                var layerMin = layer.MinY;
                if (layerMin < minY) minY = layerMin;
            }

            return Math.Min(minY, 0);
        }
    }
    
    public double MinYFilter
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
            UpdateEventVisibility();
        }
    }
    
    public double MaxYFilter
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
            UpdateEventVisibility();
        }
    }

    public void InitFilters()
    {
        MaxYFilter = MaxY;
        MinYFilter = MinY;
    }
    
    private void UpdateEventVisibility()
    {
        foreach (var layer in Layers)
        {
            foreach (var ev in layer.Events)
            {
                ev.IsWithinMinMax = ev.Y >= MinYFilter && ev.Y <= MaxYFilter;
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class Layer : INotifyPropertyChanged
{
    private static readonly Random Random = new();
    
    private static int _layerCounter;
    
    public IBrush Brush { get; set; } = ColorGeneration.Palette[Random.Next(ColorGeneration.Palette.Length)];
    public string Name { get; set; } = "Layer " + (_layerCounter++ + 1);
    public List<EventData> Events { get; set; } = [];
    
    public double MinY
    {
        get
        {
            var minY = Events[0].Y;
            foreach (var ev in Events)
            {
                if (ev.Y < minY) minY = ev.Y;
            }
            return minY;
        }
    }
    
    public double MaxY
    {
        get
        {
            var maxY = 0.0;
            foreach (var ev in Events)
            {
                if (ev.Y > maxY) maxY = ev.Y;
            }
            return maxY;
        }
    }
    
    public bool IsVisible
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
        }
    } = true;
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class ScriptRun : INotifyPropertyChanged
{
    private static int _scriptRunCounter;
    
    public string Name { get; set; } = "Script run " + (_scriptRunCounter++ + 1);
    public ObservableCollection<TimelineOutput> Outputs { get; } = [];
    public event PropertyChangedEventHandler? PropertyChanged;

    public ScriptRun()
    {
        Outputs.CollectionChanged += (sender, args) => OnPropertyChanged();
    }

    public void SetActive(EventTimelineViewportDataContext dataContext)
    {
        dataContext.SelectedScriptRun(this);
    }
    
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class EventData
{
    public int FrameIndex { get; set; }
    public double Y { get; set; }
    public List<int> TrackIds { get; set; } = [];
    public List<int> EntityIds { get; set; } = [];
    public bool IsBoolean { get; set; }
    public bool IsWithinMinMax { get; set; }

    // public BoundingBox ObjectBbox { get;} = objectBbox;
    // public bool IsFirstEventOfObject { get; set; } = isFirstEventOfObject;
    // public Entity Entity { get; } = entity;
    //
    // public override string ToString()
    // {
    //     StringBuilder sb = new();
    //     sb.Append($"Object track ID: {ObjectBbox.ClassName.TrackId}\n");
    //     sb.Append($"The first event of this object: {IsFirstEventOfObject}\n");
    //     
    //     // TODO: info about entity (maybe add entity IDs to identify them easily?)
    //     var entityType = Entity.GetType().Name;
    //     if (entityType.EndsWith("Entity")) entityType = entityType.Substring(0, entityType.Length - 6);
    //     sb.Append($"Entity: {entityType}\n");
    //    
    //     return sb.ToString();
    // }
}
