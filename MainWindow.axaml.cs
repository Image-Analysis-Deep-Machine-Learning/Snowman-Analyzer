using System;
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
using Snowman.Core.Tools;
using Ursa.Controls;
using Snowman.Data;
using Snowman.DataContexts;
using Snowman.Utilities;
using Dispatcher = Avalonia.Threading.Dispatcher;

namespace Snowman
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        
        public List<Tool> Tools { get; } = ToolRegistry.Tools;

        public static IBrush SystemColorBrush { get; private set; } = new SolidColorBrush(Color.Parse("#0078D4"));
        private readonly IBrush _brush;
        
        private string? _lastCustomZoom = null;
        
        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            SnowmanApp.Instance.Project.SelectedEntityChanged += (s, e) => OnPropertyChanged(nameof(IsEntitySelected));
            SnowmanApp.Instance.ActiveTool = Tools[0];
            
            var theme = Application.Current?.ActualThemeVariant;
            
            if (Application.Current?.FindResource(theme, "SystemAccentColor") is Color accent)
            {
                SystemColorBrush = new SolidColorBrush(accent);
            }

            _brush = new SolidColorBrush(Colors.White);
            
            SetupZoomScaleChangedHandler();
            
            UIEventBus.InfoRequested += msg =>
            {
                InfoBox.Text = msg;
            };
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

            try
            {
                await SnowmanApp.Instance.Project.OpenXml(filePickerResult[0]);
            }

            catch (Exception ex)
            {
                await MessageBox.ShowAsync("Unable to load selected file.",  "Error", MessageBoxIcon.Error);
                return;
            }
            
            Canvas.InvalidateVisual();
            FrameTimeline.InvalidateVisual();
            EventTimeline.InvalidateVisual();
        }

        public async Task OpenProject()
        {
            var filePickerResult = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                AllowMultiple = false,
                FileTypeFilter = [AdditionalFilePickerFileTypes.Xml],
                Title = "Open Project File"
            });

            if (!filePickerResult.Any()) return;

            try
            {
                await SnowmanApp.Instance.OpenProject(filePickerResult[0]);
            }

            catch (Exception ex)
            {
                await MessageBox.ShowAsync("Unable to load selected file.",  "Error", MessageBoxIcon.Error);
                return;
            }
            
            Canvas.InvalidateVisual();
            FrameTimeline.InvalidateVisual();
            EventTimeline.InvalidateVisual();
        }

        public async Task SaveProject()
        {
            var filePickerResult = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
            {
                FileTypeChoices = [AdditionalFilePickerFileTypes.Xml],
                Title = "Save Project File"
            });

            if (filePickerResult is null) return;

            try
            {
                await SnowmanApp.Instance.Project.SaveProject(filePickerResult);
            }

            catch (Exception ex)
            {
                await MessageBox.ShowAsync("Unable to load selected file.",  "Error", MessageBoxIcon.Error);
                return;
            }
            
            Canvas.InvalidateVisual();
            FrameTimeline.InvalidateVisual();
            EventTimeline.InvalidateVisual();
        }

        public void NewProject()
        {
            
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
            var ruleName = new StringBuilder();

            /*if (IsEntitySelected)
                for (var i = 0; i < SnowmanApp.Instance.Project.SelectedEntity!.Scripts.Count; i++)
                {
                    var script = SnowmanApp.Instance.Project.SelectedEntity.Scripts[i];
                    ruleName.Append(script.Name);
                    if (i != SnowmanApp.Instance.Project.SelectedEntity.Scripts.Count - 1) ruleName.Append(" + ");
                }
*/
            var (baseColor, lightColor) = ColorGeneration.GetHuePair(ruleId);
            EventTimelineDataContext.TimelineColors.TryAdd(ruleId, (baseColor, lightColor));
            
            var output = SnowmanApp.Instance.Project.Demo();
            DemoOutput.Text = output.Item1;

            SnowmanApp.Instance.Project.Rules.Add(new RuleData(ruleId, ruleName.ToString(), output.Item3));
            SnowmanApp.Instance.Project.EventsByFrameIndexByRuleId.Add(ruleId, output.Item2 ?? new Dictionary<int, List<EventData>>());
            
            SnowmanApp.Instance.EventTimelineDataContext.Redraw();
            EventTimeline.InvalidateVisual();
        }

        public void ClearEventInfo()
        {
            SnowmanApp.Instance.Project.TempEntities = null;
            SnowmanApp.Instance.Project.TempBoundingBoxes = null;
            
            SnowmanApp.Instance.CanvasDataContext.ParentRendererControl.InvalidateVisual();
            SnowmanApp.Instance.FrameTimelineDataContext.ParentRendererControl.InvalidateVisual();
            
            InfoBox.Text = string.Empty;
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

        public bool IsEntitySelected => SnowmanApp.Instance.Project.SelectedEntity is not null;

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

        public void DisplayInfo(string info)
        {
            InfoBox.Text = info;
        }
    }
}
