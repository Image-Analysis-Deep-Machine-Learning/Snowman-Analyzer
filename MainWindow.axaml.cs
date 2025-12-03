using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Snowman.Core;
using Snowman.Core.Services;
using Snowman.Core.Services.Impl;
using Snowman.DataContexts;
using Snowman.Factories;

namespace Snowman
{
    public partial class MainWindow : Window
    {
        public static readonly StyledProperty<IServiceProvider> ServiceProviderProperty =
// this warning is a lie
#pragma warning disable AVP1002
            AvaloniaProperty.Register<MainWindow, IServiceProvider>(nameof(ServiceProvider));
#pragma warning restore AVP1002
        public IServiceProvider ServiceProvider
        {
            get => GetValue(ServiceProviderProperty);
            set => SetValue(ServiceProviderProperty, value);
        }

        public static IBrush SystemColorBrush { get; private set; } = new SolidColorBrush(Color.Parse("#0078D4"));
        private readonly IBrush _brush;
        
        static MainWindow()
        {
            ServiceProviderProperty.Changed.AddClassHandler<MainWindow>((control, e) =>
            {
                if (e.NewValue is IServiceProvider provider)
                {
                    ServiceProviderScope.SetProvider(control, provider);
                    control.DataContext = new MainWindowDataContext(provider);
                }
            });
        }
        
        public MainWindow()
        {
            var serviceProvider = new ServiceProviderImpl();
            new SnowmanApp(serviceProvider); // prasačina jak delo TODO: REMOVE
            ServiceProvider = serviceProvider; // set the property later to delay creation of MainWindowDataContext which needs initialized SnowmanApp
            InitializeComponent();
            StorageProviderFactory.InitializeStorageProvider(StorageProvider);

            if (DataContext is MainWindowDataContext dataContext)
            {
                dataContext.SetupZoomScaleChangedHandler();
            }
            
            var theme = Application.Current?.ActualThemeVariant;
            
            if (Application.Current?.FindResource(theme, "SystemAccentColor") is Color accent)
            {
                SystemColorBrush = new SolidColorBrush(accent);
            }

            _brush = new SolidColorBrush(Colors.White);
            
            // UIEventBus.InfoRequested += msg =>
            // {
            //     InfoBox.Text = msg;
            // };
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
}
