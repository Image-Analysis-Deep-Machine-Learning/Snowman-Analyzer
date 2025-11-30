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
        public IServiceProvider ServiceProvider { get; private set; }
        public new event PropertyChangedEventHandler? PropertyChanged;

        public static IBrush SystemColorBrush { get; private set; } = new SolidColorBrush(Color.Parse("#0078D4"));
        private readonly IBrush _brush;
        
        public MainWindow()
        {
            ServiceProvider = new SnowmanServiceProvider();
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
