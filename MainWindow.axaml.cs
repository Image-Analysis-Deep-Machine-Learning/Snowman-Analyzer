using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Snowman.Core;

namespace Snowman
{
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Core of the app containing all data and providing methods for their manipulation from the main GUI
        /// </summary>
        public SnowmanApp CoreApp { get; }

        public MainWindow()
        {
            DataContext = this;
            CoreApp = new SnowmanApp();
            InitializeComponent();
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
            await CoreApp.Project.LoadVideoFile(result[0], ownerWindow);
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
        }

        public void PrevFrame()
        {
            CoreApp.Project.PreviousFrame();
            WorkingAreaRenderer.InvalidateVisual();
        }
        
        public void NextFrame()
        {
            CoreApp.Project.NextFrame();
            WorkingAreaRenderer.InvalidateVisual();
        }
    }
}
