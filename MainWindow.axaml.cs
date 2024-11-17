using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Snowman
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click_2(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
