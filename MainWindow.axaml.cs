using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Snowman.Core;
using Snowman.Core.Entities;
using Snowman.Core.Services;
using Snowman.Core.Services.Impl;
using Snowman.Core.Tools;
using Ursa.Controls;
using Snowman.Data;
using Snowman.DataContexts;
using Snowman.Factories;
using Snowman.Utilities;
using Dispatcher = Avalonia.Threading.Dispatcher;

namespace Snowman
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public static readonly StyledProperty<IServiceProvider> ServiceProviderProperty =
// this warning is a lie
#pragma warning disable AVP1002
            AvaloniaProperty.Register<MainWindow, IServiceProvider>(nameof(ServiceProvider));
#pragma warning restore AVP1002

        /// <summary>
        /// Service provider property that must be set if this UserControl needs access to it
        /// </summary>
        public IServiceProvider ServiceProvider
        {
            get => GetValue(ServiceProviderProperty);
            set => SetValue(ServiceProviderProperty, value);
        }
        public new event PropertyChangedEventHandler? PropertyChanged;

        public static IBrush SystemColorBrush { get; private set; } = new SolidColorBrush(Color.Parse("#0078D4"));
        private readonly IBrush _brush;
        
        static MainWindow()
        {
            ServiceProviderProperty.Changed.AddClassHandler<MainWindow>((toolBar, e) =>
            {
                if (e.NewValue is IServiceProvider provider)
                {
                    toolBar.DataContext = new MainWindowDataContext(provider);
                }
            });
        }
        
        public MainWindow()
        {
            ServiceProvider = new ServiceProviderImpl();
            SnowmanApp._instance = new SnowmanApp(ServiceProvider); // prasačina jak delo
            InitializeComponent();
            StorageProviderFactory.InitializeStorageProvider(StorageProvider);

            if (DataContext is MainWindowDataContext dataContext)
            {
                dataContext.SetupZoomScaleChangedHandler();
            }
            SnowmanApp.Instance.Project.SelectedEntityChanged += (s, e) => OnPropertyChanged("IsEntitySelected");
            
            var theme = Application.Current?.ActualThemeVariant;
            
            if (Application.Current?.FindResource(theme, "SystemAccentColor") is Color accent)
            {
                SystemColorBrush = new SolidColorBrush(accent);
            }

            _brush = new SolidColorBrush(Colors.White);
            
            UIEventBus.InfoRequested += msg =>
            {
                InfoBox.Text = msg;
            };
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
