using Avalonia;

namespace Snowman.Core.Services;

public class ServiceProviderAttachedProperty
{
    public static readonly AttachedProperty<IServiceProvider> ProviderProperty =
        AvaloniaProperty.RegisterAttached<ServiceProviderAttachedProperty, AvaloniaObject, IServiceProvider>(
            "Provider",
            inherits: true);

    public static void SetProvider(AvaloniaObject o, IServiceProvider value) =>
        o.SetValue(ProviderProperty, value);

    public static IServiceProvider GetProvider(AvaloniaObject o) =>
        o.GetValue(ProviderProperty);
}
