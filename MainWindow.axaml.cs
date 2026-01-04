using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Snowman.Core;
using Snowman.Core.Services;
using Snowman.Core.Services.Impl;
using Snowman.DataContexts;
using Snowman.Factories;

namespace Snowman;

public partial class MainWindow : Window
{
    private static IBrush SystemColorBrush { get; set; } = new SolidColorBrush(Color.Parse("#0078D4"));
    private readonly IBrush _brush;
    private SnowmanApp _app;

    static MainWindow()
    {
        DataContextProperty.Changed.AddClassHandler<MainWindow>((window, args) =>
        {
            if (args.NewValue is not MainWindowDataContext dataContext) return;
            
            dataContext.Constructor(ServiceProviderAttachedProperty.GetProvider(window));
            //dataContext.SetupZoomScaleChangedHandler();
        });
    }

    public MainWindow()
    {
        ServiceProviderAttachedProperty.SetProvider(this, new ServiceProviderImpl());
        _app = new SnowmanApp(ServiceProviderAttachedProperty.GetProvider(this));
        StorageProviderFactory.InitializeStorageProvider(StorageProvider);
        InitializeComponent();
        
        var theme = Application.Current?.ActualThemeVariant;
            
        if (Application.Current?.FindResource(theme, "SystemAccentColor") is Color accent)
        {
            SystemColorBrush = new SolidColorBrush(accent);
        }

        _brush = new SolidColorBrush(Colors.White);
    }

    private void ToggleFrameTimelineButton_OnClick(object? sender, RoutedEventArgs e)
    {
        FrameTimelineGrid.IsVisible = true;
        FrameTimelinePath.Fill = SystemColorBrush;
        EventTimelineBorder.IsVisible = false;
        EventTimelinePath.Fill = _brush;
        ZoomComboBox.IsVisible = false;
    }

    private void ToggleEventTimelineButton_OnClick(object? sender, RoutedEventArgs e)
    {
        EventTimelineBorder.IsVisible = true;
        EventTimelinePath.Fill = SystemColorBrush;
        FrameTimelineGrid.IsVisible = false;
        FrameTimelinePath.Fill = _brush;
        ZoomComboBox.IsVisible = true;
    }
}