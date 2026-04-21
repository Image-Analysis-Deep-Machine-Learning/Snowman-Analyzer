using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Python.Runtime;
using Snowman.Core.Services;
using Snowman.Core.Services.Impl;
using Snowman.Core.Settings;
using Snowman.DataContexts;
using Snowman.Utilities;
using Ursa.Controls;

namespace Snowman.Windows;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializePythonExecutionEnvironment();
        // the most important lines of this application that handle the dependency injection magic
        var serviceProvider = new ServiceProviderImpl();
        serviceProvider.RegisterService<IStorageProviderService>(new StorageProviderServiceImpl(StorageProvider));
        DataContext = new MainWindowDataContext(serviceProvider);
        ServiceProviderAttachedProperty.SetProvider(this, serviceProvider);

        InitializeComponent();
        
        serviceProvider.RegisterService<IProgressBarService>(new ProgressBarServiceImpl(ProgressBar, ProgressBarText));
        serviceProvider.RegisterService<ILoggerService>(new LoggerServiceImpl(LoggerTextBox));
    }
    
    public async Task LoadVideoFile()
    {
        try
        {
            var serviceProvider = ServiceProviderAttachedProperty.GetProvider(this);
            var metadataResult = await new LoadVideoWindow(serviceProvider).ShowDialog<VideoSequenceMetadata?>(this);

            if (metadataResult is null) return;
            
            var newDatasetPath = await VideoFileLoader.ExtractFramesAsync(metadataResult, serviceProvider);
            await serviceProvider.GetService<IProjectService>().OpenDataset(newDatasetPath);
        }

        catch (Exception e)
        {
            await MessageBox.ShowAsync("Cannot load the video " + e.Message);
        }
    }

    public async Task RunMot()
    {
        try
        {
            var serviceProvider = ServiceProviderAttachedProperty.GetProvider(this);
            await new MultiObjectTrackingWindow(serviceProvider).ShowDialog(this);
        }
        
        catch (Exception e)
        {
            await MessageBox.ShowAsync("Cannot run mot: " + e.Message);
        }
    }

    public void OpenChat()
    {
        var serviceProvider = ServiceProviderAttachedProperty.GetProvider(this);
        var chatService = serviceProvider.GetService<IChatService>();
        chatService.OpenChatWindow();
    }

    protected override void OnClosed(EventArgs e)
    {
        var serviceProvider = ServiceProviderAttachedProperty.GetProvider(this);
        var chatService = serviceProvider.GetService<IChatService>();
        chatService.CloseChatWindow();
    }

    private static void InitializePythonExecutionEnvironment()
    {
        if (Design.IsDesignMode) return; // do not initialize PythonEngine in the design mode to prevent crashes

        try
        {
            Runtime.PythonDLL = SettingsRegistry.PythonDllPath.Value;
            PythonEngine.PythonHome = SettingsRegistry.PythonHomeDirectory.Value;
            PythonEngine.Initialize();
            PythonEngine.BeginAllowThreads();
        }

        catch (Exception e)
        {
            MessageBox.ShowAsync($"Cannot initialize Python engine. Check if the Python Dll Path in Settings is valid.\n{e.Message}\n{e.StackTrace}", "Error", MessageBoxIcon.Error);
        }
    }
}
