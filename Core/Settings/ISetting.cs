using System.Collections.Generic;
using System.Collections.ObjectModel;
using Snowman.Events;

namespace Snowman.Core.Settings;

public interface ISetting<T> : ISetting
{
    public event EventHandler<T>? ValueChanged;
    public T Value { get; set; }
    public ObservableCollection<T>? AllowedValues { get; init; }
}

public interface ISetting
{
    public object BoxedValue { get; set; }
    public ObservableCollection<object>? BoxedAllowedValues { get; init; }
}
