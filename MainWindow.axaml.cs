using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Python.Runtime;
using Snowman.Core;
using Snowman.Core.Entities;
using Snowman.Core.Tools;
using Snowman.Data;
using Snowman.DataContexts;
using Snowman.Utilities;
using Dispatcher = Avalonia.Threading.Dispatcher;

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
        
        private string? _lastCustomZoom = null;

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
            
            SetupZoomScaleChangedHandler();
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
            EventTimeline.InvalidateVisual();
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
            EventTimeline.InvalidateVisual();
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

            var ruleId = SnowmanApp.Instance.Project.Rules.Count; 
            SnowmanApp.Instance.Project.Rules.Add(new RuleData(ruleId, "Point intersection"));
            
            var (baseColor, lightColor) = ColorGeneration.GetColorPair(ruleId, SnowmanApp.Instance.Project.Rules.Count);
            EventTimelineDataContext.TimelineColors.TryAdd(ruleId, (baseColor, lightColor));
            
            if (ruleId == 0)
                SnowmanApp.Instance.EventTimelineDataContext.EventsByRuleId.Add(ruleId, GetRuleOutputEvents1());
            else if (ruleId == 1)
                SnowmanApp.Instance.EventTimelineDataContext.EventsByRuleId.Add(ruleId, GetRuleOutputEvents2());
            else
                SnowmanApp.Instance.EventTimelineDataContext.EventsByRuleId.Add(ruleId, GetRuleOutputEvents3());
            
            SnowmanApp.Instance.EventTimelineDataContext.Redraw();
            EventTimeline.InvalidateVisual();
            
            //var output = SnowmanApp.Instance.Project.Demo();
            //DemoOutput.Text = output;
        }

        private List<EventData> GetRuleOutputEvents1()
        {
            var point1 = new PointEntity(new Point(285, 265));
            var className1 = new ClassName { TrackId = 1 };
            
            var bbox11 = new BoundingBox { XLeftTop = 250, YLeftTop = 220, Height = 50, Width = 50, ClassName = className1 };
            
            var bbox12 = new BoundingBox { XLeftTop = 260, YLeftTop = 230, Height = 50, Width = 50, ClassName = className1 };
            
            var bbox13 = new BoundingBox { XLeftTop = 270, YLeftTop = 240, Height = 50, Width = 50, ClassName = className1 };
            
            List<EventData> outputEventsRule1 = [
                new(0, bbox11, true, point1, 0),
                new(1, bbox12, false, point1, 0),
                new(2, bbox13, false, point1, 0),
            ];

            return outputEventsRule1;
        }
        
        private List<EventData> GetRuleOutputEvents2()
        {
            var point2 = new PointEntity(new Point(300, 300));
            var className1 = new ClassName { TrackId = 1 };
            var className2 = new ClassName { TrackId = 2 };
            
            var bbox14 = new BoundingBox { XLeftTop = 280, YLeftTop = 260, Height = 50, Width = 50, ClassName = className1 };
            
            var bbox21 = new BoundingBox { XLeftTop = 290, YLeftTop = 290, Height = 40, Width = 60, ClassName = className2 };
            
            var bbox22 = new BoundingBox { XLeftTop = 290, YLeftTop = 298, Height = 40, Width = 60, ClassName = className2 };
            
            var bbox23 = new BoundingBox { XLeftTop = 298, YLeftTop = 290, Height = 40, Width = 60, ClassName = className2 };
            
            List<EventData> outputEventsRule2 =
            [
                new(3, bbox14, true, point2, 1),
                new(10, bbox21, true, point2, 1),
                new(11, bbox22, false, point2, 1),
                new(12, bbox23, false, point2, 1),
            ];

            return outputEventsRule2;
        }

        private List<EventData> GetRuleOutputEvents3()
        {
            var point3 = new PointEntity(new Point(500, 100));
            var className3 = new ClassName { TrackId = 3 };
            
            var bbox31 = new BoundingBox { XLeftTop = 450, YLeftTop = 50, Height = 70, Width = 40, ClassName = className3 };
            
            var bbox32 = new BoundingBox { XLeftTop = 460, YLeftTop = 55, Height = 70, Width = 40, ClassName = className3 };
            
            var bbox33 = new BoundingBox { XLeftTop = 470, YLeftTop = 60, Height = 70, Width = 40, ClassName = className3 };
            
            var bbox34 = new BoundingBox { XLeftTop = 480, YLeftTop = 70, Height = 70, Width = 40, ClassName = className3 };
            
            List<EventData> outputEventsRule3 =
            [
                new(20, bbox31, true, point3, 2),
                new(21, bbox32, false, point3, 2),
                new(25, bbox33, false, point3, 2),
                new(26, bbox34, false, point3, 2),
            ];

            return outputEventsRule3;
        }
        
        public ObservableCollection<string> ZoomScaleOptions { get; } = ["1x", "2x", "5x", "10x", "20x"];
        private static string FormatZoomScale(double value) => $"{value:0.#}x";
        public string ZoomScaleString
        {
            get => FormatZoomScale(SnowmanApp.Instance.EventTimelineDataContext.ZoomScale);
            set
            {
                if (value.EndsWith('x') && double.TryParse(value.TrimEnd('x'), out var parsed))
                {
                    SnowmanApp.Instance.EventTimelineDataContext.ZoomScale = parsed;
                    OnPropertyChanged();
                    EventTimeline.InvalidateVisual();
                }
            }
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
        
        private void SetupZoomScaleChangedHandler()
        {
            SnowmanApp.Instance.EventTimelineDataContext.ZoomScaleChanged += () =>
            {
                var zoomScale = FormatZoomScale(SnowmanApp.Instance.EventTimelineDataContext.ZoomScale);

                if (!ZoomScaleOptions.Contains(zoomScale))
                {
                    if (_lastCustomZoom is not null) ZoomScaleOptions.Remove(_lastCustomZoom);
                    ZoomScaleOptions.Add(zoomScale);
                    _lastCustomZoom = zoomScale;
                }
                else if (_lastCustomZoom != null && zoomScale != _lastCustomZoom)
                {
                    var toRemove = _lastCustomZoom;
                    _lastCustomZoom = null;
                    // remove the temporary zoom scale after UI update finishes
                    Dispatcher.UIThread.Post(() =>
                    {
                        ZoomScaleOptions.Remove(toRemove);
                    }, Avalonia.Threading.DispatcherPriority.Background);
                }
                
                OnPropertyChanged(nameof(ZoomScaleString));
            };
        }
    }
}
