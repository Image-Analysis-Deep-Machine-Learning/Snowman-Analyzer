using System;
using Avalonia;
using Avalonia.Controls;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.Controls;

/// <summary>
/// UserControl wrapper that makes the life easier by adding a ServiceProvider property and the ability to strongly type
/// the DataContext without casting and null-checking
/// </summary>
/// <typeparam name="T">Type of the DataContext</typeparam>
public class ServiceableUserControl<T> : UserControl where T : class
{
    public static readonly StyledProperty<IServiceProvider> ServiceProviderProperty =
// this warning is a lie
#pragma warning disable AVP1002
        AvaloniaProperty.Register<ServiceableUserControl<T>, IServiceProvider>(nameof(ServiceProvider));
#pragma warning restore AVP1002

    /// <summary>
    /// Service provider property that must be set if this UserControl needs access to it
    /// </summary>
    public IServiceProvider ServiceProvider
    {
        get => GetValue(ServiceProviderProperty);
        set => SetValue(ServiceProviderProperty, value);
    }

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
}