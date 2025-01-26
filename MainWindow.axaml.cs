using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Snowman.Controls;
using Snowman.Core;
using Snowman.DataContexts;

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
    }
}
