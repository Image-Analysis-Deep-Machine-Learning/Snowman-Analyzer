using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Snowman.Core;
using Snowman.Core.Entities;
using Snowman.Core.Tools;

namespace Snowman
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        
        public static ViewportMoveTool MoveTool { get; } = new();
        public static EntityEditTool<Entity> EntityEditTool { get; } = new();
        public static PointTool PointTool { get; } = new();
        public static RectTool RectTool { get; } = new();

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
            Timeline.InvalidateVisual();
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
            Timeline.InvalidateVisual();
        }

        public void PrevFrame()
        {
            SnowmanApp.Instance.Project.PreviousFrame();
            Canvas.InvalidateVisual();
            Timeline.InvalidateVisual();
        }
        
        public void NextFrame()
        {
            SnowmanApp.Instance.Project.NextFrame();
            Canvas.InvalidateVisual();
            Timeline.InvalidateVisual();
        }

        public void UpdateFrame()
        {
            Canvas.InvalidateVisual();
            Timeline.InvalidateVisual();
        }

        public void Demo()
        {
            if (Design.IsDesignMode) return;
            
            var output = SnowmanApp.Instance.Project.Demo();
            DemoOutput.Text = output;
        }
                
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
