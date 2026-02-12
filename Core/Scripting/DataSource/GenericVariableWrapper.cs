using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Snowman.Core.Scripting.DataSource;

public abstract class GenericVariableWrapper<T> : Variable, INotifyPropertyChanged
{
    public T? TypedValue
    {
        get => (T?)Value;

        set
        {
            Value = value;
            OnPropertyChanged();
        }
    }

    protected GenericVariableWrapper(string name, Group group, string friendlyName) : base(name, typeof(T), group, friendlyName)
    {
        
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<TParam>(ref TParam field, TParam value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<TParam>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
