using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Media;
using Snowman.DataContexts;

namespace Snowman.Data;

public class TimelineOutput
{
    public string Name { get; set; }
    public List<Layer> Layers { get; set; } = [];
}

public class Layer
{
    public List<EventData> Events { get; set; } = [];
    public double Min { get; set; }
    public double Max { get; set; }
    public string Name { get; set; }
    public IBrush Brush { get; set; } = Brushes.Chocolate;
    public bool IsVisible { get; set; } = true;
}

public class ScriptRun : INotifyPropertyChanged
{

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<TimelineOutput> Outputs { get; } = [];
    public string Name { get; set; }

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

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

public class EventData
{
    public int FrameIndex { get; set; }
    public double Y { get; set; }
    public List<int> TrackIds { get; set; } = [];
    public List<int> EntityIds { get; set; } = [];
    public bool IsBoolean { get; set; }

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