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

namespace Snowman
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public static Tool MoveTool => Tool.MoveTool;
        public static Tool PointTool => Tool.PointTool;

        /// <summary>
        /// Core of the app containing all data and providing methods for their manipulation from the main GUI
        /// </summary>
        public SnowmanApp CoreApp { get; }

        public string CurrentStringPath
        {
            get => CoreApp.Project.SelectedEntity is null ? string.Empty : CoreApp.Project.SelectedEntity.ScriptPath;

            set
            {
                if (CoreApp.Project.SelectedEntity is null) return;
                CoreApp.Project.SelectedEntity.ScriptPath = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            CoreApp = new SnowmanApp(this);
            CoreApp.Project.SelectedEntityChanged += (s, e) => OnPropertyChanged(nameof(CurrentStringPath));
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
            await CoreApp.Project.LoadVideoFile(result[0], ownerWindow, ProgressBar, ProgressBarText);
            
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
            
            CoreApp.Project.OpenXml(result.First());
            
            WorkingAreaRenderer.InvalidateVisual();
            TimelineRenderer.InvalidateVisual();
        }

        public void PrevFrame()
        {
            CoreApp.Project.PreviousFrame();
            WorkingAreaRenderer.InvalidateVisual();
            TimelineRenderer.InvalidateVisual();
        }
        
        public void NextFrame()
        {
            CoreApp.Project.NextFrame();
            WorkingAreaRenderer.InvalidateVisual();
            TimelineRenderer.InvalidateVisual();
        }

        public void UpdateFrame()
        {
            WorkingAreaRenderer.InvalidateVisual();
            TimelineRenderer.InvalidateVisual();
        }

        public void SetTool(Tool tool) => CoreApp.ActiveTool = tool;

        public void Demo()
        {
            if (Design.IsDesignMode) return;
            
            var output = CoreApp.Project.Demo();
            DemoOutput.Text = output;
        }
    }
}
