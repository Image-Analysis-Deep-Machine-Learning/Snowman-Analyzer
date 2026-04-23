using System;
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
    private readonly IMessageBoxService _messageBoxService;
    
    public MainWindow()
    {
        InitializePythonExecutionEnvironment();
        // the most important lines of this application that handle the dependency injection magic
        var serviceProvider = new ServiceProviderImpl();
        serviceProvider.RegisterService<IStorageProviderService>(new StorageProviderServiceImpl(StorageProvider));
        _messageBoxService = new MessageBoxServiceImpl(this);
        serviceProvider.RegisterService(_messageBoxService);
        serviceProvider.RegisterService<IChatService>(new ChatServiceImplementation(serviceProvider));
        DataContext = new MainWindowDataContext(serviceProvider);
        ServiceProviderAttachedProperty.SetProvider(this, serviceProvider);

        InitializeComponent();
        
        serviceProvider.RegisterService<IProgressBarService>(new ProgressBarServiceImpl(ProgressBar, ProgressBarText));
        serviceProvider.RegisterService<ILoggerService>(new LoggerServiceImpl(LoggerTextBox));
    }
    
    public async Task LoadVideoFile()
    {
        if (Environment.OSVersion.Platform != PlatformID.Win32NT)
        {
            _messageBoxService.ShowMessageBox("Error", "Loading video file is not supported on this operating system.", MessageBoxIcon.Error);
            return;
        }
        
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
            _messageBoxService.ShowMessageBox("Error", $"Cannot load video file: {e.Message}", MessageBoxIcon.Error);
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
            _messageBoxService.ShowMessageBox("Error", $"Cannot run mot: {e.Message}", MessageBoxIcon.Error);
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

    private void InitializePythonExecutionEnvironment()
    {
        if (Design.IsDesignMode) return; // do not initialize PythonEngine in the design mode to prevent crashes

        try
        {
            // there is literally no way of catching Python initialization errors without subprocess black magic, Snowman just crashes even inside try-catch
            if (SettingsRegistry.PythonLibraryPath.Value.EndsWith(".dll") && Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                throw new Exception(
                    "Cannot use DLL library on UNIX-like operating systems. " +
                    "Open settings, change ALL THREE Python paths to the library, executable and Python home and restart. " +
                    "Setting incorrect values may crash Snowman on startup without warning. Change the settings.json file in that case.");
            }
            
            Runtime.PythonDLL = SettingsRegistry.PythonLibraryPath.Value;
            PythonEngine.PythonHome = SettingsRegistry.PythonHomeDirectory.Value;
            PythonEngine.Initialize();
            PythonEngine.BeginAllowThreads();
        }

        catch (Exception e)
        {
            _messageBoxService.ShowMessageBox("Error", $"Cannot initialize Python engine. Check if the Python paths in Settings are valid.\n{e.Message}\n{e.StackTrace}", MessageBoxIcon.Error);
        }
    }
}
