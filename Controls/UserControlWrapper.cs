using System;
using Avalonia;
using Avalonia.Controls;
using Snowman.Core.Services;
using Snowman.DataContexts;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.Controls;

/// <summary>
/// UserControl wrapper that makes life easier by adding the ability to strongly type the DataContext without casting and null-checking
/// </summary>
/// <typeparam name="T">Type of the DataContext</typeparam>
public abstract class UserControlWrapper<T> : UserControl where T : class, new()
{
    /// <summary>
    /// A replacement for default DataContext that needs to be cast AND null-checked EVERY FUCKING TIME before use.
    /// Shouldn't be the purpose of a XAML UI framework reducing unnecessary UI boilerplate?
    /// </summary>
    /// <exception cref="NullReferenceException">thrown when the getter is accessed but the underlying DataContext is not set yet</exception>
    public new T DataContext
    {
        get => base.DataContext as T ?? throw new NullReferenceException($"{nameof(DataContext)} cannot be null when accessed.");
        set => base.DataContext = value;
    }

    static UserControlWrapper()
    {
        ServiceProviderAttachedProperty.ProviderProperty.Changed.AddClassHandler<UserControlWrapper<T>>((control, args) =>
        {
            if (args.NewValue is IServiceProvider serviceProvider)
            {
                DataContextInjector.TryInjectDataContext(control, serviceProvider);
            }
        });
    }
    
    protected UserControlWrapper()
    {
        DataContext = new T();
    }
}
