using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Python.Runtime;
using Snowman.Core.Services;
using Snowman.Core.Services.Impl;
using Snowman.DataContexts;
using Snowman.Utilities;
using Ursa.Controls;

namespace Snowman;

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
            var metadataResult = await new Controls.LoadVideoWindow(serviceProvider).ShowDialog<VideoSequenceMetadata?>(this);

            if (metadataResult is null) return;
            
            var newDatasetPath = await VideoFileLoader.ExtractFramesAsync(metadataResult, serviceProvider);
            await serviceProvider.GetService<IProjectService>().OpenDataset(newDatasetPath);
        }

        catch (Exception e)
        {
            await MessageBox.ShowAsync("Něco se posralo " + e.Message);
        }
    }
    
    private static void InitializePythonExecutionEnvironment()
    {
        if (Avalonia.Controls.Design.IsDesignMode) return; // do not initialize PythonEngine in the design mode to prevent crashes
            
        // TODO: bundle embedded python environment for Linux from https://github.com/lmbelo/python3-embeddable/ and who knows where for macOS
        var pythonDir = Path.Combine(Environment.CurrentDirectory, "python_win64");
        Runtime.PythonDLL = Path.Combine(pythonDir, "python312.dll"); 
        PythonEngine.PythonHome = pythonDir;
        PythonEngine.Initialize();
        PythonEngine.BeginAllowThreads();
            
        // TODO: all python projects (DeepSORT/Ultralytics YOLO/ByteTrack/YOLO JDE...) must offer a way to install all required libraries
        // TODO: one possible solution is to create another github frankenstein project which will include all these projects in one single place to use here
        // TODO: then Snowman should provide a framework to select a python env. (with default being the Windows' NuGet package) and install all dependencies
        // TODO: DEBUGGER
        var p = new Process();
        var exe = Path.Combine(pythonDir, "python.exe");
        p.StartInfo.FileName = exe;
        //p.StartInfo.Arguments = "-m pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu128";
        p.StartInfo.Arguments = "-m pip install matplotlib PyQt5 pyside6";
        //p.Start();
    }
}
