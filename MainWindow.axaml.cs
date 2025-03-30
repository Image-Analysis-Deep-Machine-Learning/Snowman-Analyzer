using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Snowman.Controls;
using Snowman.Core;
using Snowman.Core.Tools;

namespace Snowman
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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
            SnowmanApp.Instance.ActiveTool = new PointTool(1, default);
        }

        public async Task LoadVideoFile()
        {
            var result = StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                AllowMultiple = false,
                FileTypeFilter = [AdditionalFilePickerFileTypes.Video],
                Title = "Open Video File"
            }).Result;

            if (!result.Any()) return;

            var ownerWindow = this;
            await SnowmanApp.Instance.Project.LoadVideoFile(result[0], ownerWindow, ProgressBar, ProgressBarText);
            
            WorkingAreaRenderer.InvalidateVisual();
            TimelineRenderer.InvalidateVisual();
        }

        public void OpenXml()
        {
            var result = StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                AllowMultiple = false,
                FileTypeFilter = [AdditionalFilePickerFileTypes.Xml],
                Title = "Open XML File"
            }).Result;

            if (!result.Any()) return;
            
            SnowmanApp.Instance.Project.OpenXml(result.First());
            
            WorkingAreaRenderer.InvalidateVisual();
            TimelineRenderer.InvalidateVisual();
        }

        public void PrevFrame()
        {
            SnowmanApp.Instance.Project.PreviousFrame();
            WorkingAreaRenderer.InvalidateVisual();
            TimelineRenderer.InvalidateVisual();
        }
        
        public void NextFrame()
        {
            SnowmanApp.Instance.Project.NextFrame();
            WorkingAreaRenderer.InvalidateVisual();
            TimelineRenderer.InvalidateVisual();
        }

        public void UpdateFrame()
        {
            WorkingAreaRenderer.InvalidateVisual();
            TimelineRenderer.InvalidateVisual();
        }

        public void SetTool(Tool tool) => SnowmanApp.Instance.ActiveTool = tool;

        public void Demo()
        {
            if (Design.IsDesignMode) return;
            
            var output = SnowmanApp.Instance.Project.Demo();
            DemoOutput.Text = output;
        }
    }
}
