using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Snowman.Core;
using Snowman.Core.Entities;
using Snowman.Core.Tools;
using Snowman.Data;

namespace Snowman
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        
        public static ViewportMoveTool MoveTool { get; } = new();
        public static EntityEditTool<Entity> EntityEditTool { get; } = new();
        public static PointTool PointTool { get; } = new();
        public static RectTool RectTool { get; } = new();

        public static IBrush SystemColorBrush { get; private set; } = new SolidColorBrush(Color.Parse("#0078D4"));
        private readonly IBrush _brush;

        public string CurrentStringPath
        {
            get => SnowmanApp.Instance.Project.SelectedEntity is null ? string.Empty : SnowmanApp.Instance.Project.SelectedEntity.ScriptPath;

            set
            {
                if (SnowmanApp.Instance.Project.SelectedEntity is null) return;
                SnowmanApp.Instance.Project.SelectedEntity.ScriptPath = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            SnowmanApp.Instance.Project.SelectedEntityChanged += (s, e) => OnPropertyChanged(nameof(CurrentStringPath));
            SnowmanApp.Instance.ActiveTool = MoveTool;
            
            var theme = Application.Current?.ActualThemeVariant;
            
            if (Application.Current?.FindResource(theme, "SystemAccentColor") is Color accent)
            {
                SystemColorBrush = new SolidColorBrush(accent);
            }

            _brush = new SolidColorBrush(Colors.White);
        }

        public void SetTool(Tool tool) => SnowmanApp.Instance.ActiveTool = tool;

        public async Task LoadVideoFile()
        {
            var filePickerResult = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                AllowMultiple = false,
                FileTypeFilter = [AdditionalFilePickerFileTypes.Video],
                Title = "Open Video File"
            });

            if (!filePickerResult.Any()) return;

            var ownerWindow = this;
            await SnowmanApp.Instance.Project.LoadVideoFile(filePickerResult[0], ownerWindow, ProgressBar, ProgressBarText);
            
            Canvas.InvalidateVisual();
            FrameTimeline.InvalidateVisual();
        }

        public async Task OpenXml()
        {
            var filePickerResult = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                AllowMultiple = false,
                FileTypeFilter = [AdditionalFilePickerFileTypes.Xml],
                Title = "Open XML File"
            });

            if (!filePickerResult.Any()) return;
            
            await SnowmanApp.Instance.Project.OpenXml(filePickerResult[0]);
            
            Canvas.InvalidateVisual();
            FrameTimeline.InvalidateVisual();
        }

        public void PrevFrame()
        {
            SnowmanApp.Instance.Project.PreviousFrame();
            Canvas.InvalidateVisual();
            FrameTimeline.InvalidateVisual();
        }
        
        public void NextFrame()
        {
            SnowmanApp.Instance.Project.NextFrame();
            Canvas.InvalidateVisual();
            FrameTimeline.InvalidateVisual();
        }

        public void UpdateFrame()
        {
            Canvas.InvalidateVisual();
            FrameTimeline.InvalidateVisual();
        }

        public void Demo()
        {
            if (Design.IsDesignMode) return;

            List<EventData> outputEvents = [
                new([10, 11, 12, 13, 14, 15], 0, 0, null),
                new([0, 1, 2], 0, 0, null),
                new([20, 21, 22, 30, 31, 32, 33], 0, 0, null),
                new([145, 146, 147, 148, 149, 150], 0, 0, null)
            ];
            SnowmanApp.Instance.EventTimelineDataContext.Events = outputEvents;
            EventTimeline.InvalidateVisual();
            
            //var output = SnowmanApp.Instance.Project.Demo();
            //DemoOutput.Text = output;
        }
                
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ToggleFrameTimelineButton_OnClick(object? sender, RoutedEventArgs e)
        {
            FrameTimelineGrid.IsVisible = true;
            FrameTimelinePath.Fill = SystemColorBrush;
            EventTimelineBorder.IsVisible = false;
            EventTimelinePath.Fill = _brush;
        }

        private void ToggleEventTimelineButton_OnClick(object? sender, RoutedEventArgs e)
        {
            EventTimelineBorder.IsVisible = true;
            EventTimelinePath.Fill = SystemColorBrush;
            FrameTimelineGrid.IsVisible = false;
            FrameTimelinePath.Fill = _brush;
        }
    }
}
