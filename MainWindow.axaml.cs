using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Snowman.Core;
using Snowman.Core.Services;
using Snowman.Core.Services.Impl;
using Snowman.DataContexts;

namespace Snowman;

public partial class MainWindow : Window
{
    private static IBrush SystemColorBrush { get; set; } = new SolidColorBrush(Color.Parse("#0078D4"));
    private readonly IBrush _brush;
    private SnowmanApp _app;

    public MainWindow()
    {
        // the most important lines of this application that handle the dependency injection magic
        var serviceProvider = new ServiceProviderImpl();
        DataContext = new MainWindowDataContext(serviceProvider);
        ServiceProviderAttachedProperty.SetProvider(this, serviceProvider);
        
        serviceProvider.RegisterService<IStorageProviderService>(new StorageProviderServiceImpl(StorageProvider));
        _app = new SnowmanApp(serviceProvider);
        
        InitializeComponent();
        
        serviceProvider.RegisterService<ILoggerService>(new LoggerServiceImpl(LoggerTextBox));
        
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
