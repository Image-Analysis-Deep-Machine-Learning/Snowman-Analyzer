using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Snowman.Core.Services;
using Snowman.Core.Settings;
using Snowman.Events;
using Snowman.Utilities;
using Ursa.Controls;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.DataContexts;

public partial class MultiObjectTrackingWindowDataContext : INotifyPropertyChanged
{
    private static readonly List<Detector> Detectors =
    [
        new("RTDETR", ["rtdetr-l.pt", "rtdetr-x.pt"]),
        new("YOLOv26", ["yolo26n.pt", "yolo26s.pt", "yolo26m.pt", "yolo26l.pt", "yolo26x.pt"])
    ];

    private readonly IStorageProviderService _storageProviderService;
    private readonly IProgressBarService _progressBarService;
    private readonly IProjectService _projectService;
    private readonly ILoggerService _loggerService = null!;

    public static IEnumerable<string> AvailableDetectors => Detectors.Select(x => x.Name);
    public static IEnumerable<string> AvailableTrackers => ["botsort", "bytetrack"];
    
    public event PropertyChangedEventHandler? PropertyChanged;
    public event SignalEventHandler? DialogCloseRequested;

    public string? VideoFilePath
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string? OutputFolderPath
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string SelectedDetector
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(AvailableModels));
            SelectedModel = AvailableModels.First();
        }
    }

    public string SelectedModel
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string SelectedTracker
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }
    
    public IEnumerable<string> AvailableModels => Detectors.First(x => x.Name == SelectedDetector).Weights;

    public MultiObjectTrackingWindowDataContext(IServiceProvider serviceProvider)
    {
        _storageProviderService = serviceProvider.GetService<IStorageProviderService>();
        _progressBarService = serviceProvider.GetService<IProgressBarService>();
        _projectService = serviceProvider.GetService<IProjectService>();
        _loggerService = serviceProvider.GetService<ILoggerService>();
        SelectedDetector = Detectors.First().Name;
        SelectedModel = AvailableModels.First();
        SelectedTracker = AvailableTrackers.First();
    }

    public void InitializeRequiredLibraries()
    {
        // TODO: all python projects (DeepSORT/Ultralytics YOLO/ByteTrack/YOLO JDE...) must offer a way to install all required libraries
        // TODO: one possible solution is to create another github frankenstein project which will include all these projects in one single place to use here
        // TODO: then Snowman should provide a framework to select a python env. (with default being the Windows' NuGet package) and install all dependencies
        // TODO: DEBUGGER
        var p = new Process();
        var executable = SettingsRegistry.PythonExecutablePath.Value;
        p.StartInfo.FileName = executable;
        //p.StartInfo.Arguments = "-m pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu128";
        //p.StartInfo.Arguments = "-m pip install matplotlib PyQt5 pyside6";
        // TODO: GPU ACCELERATION
        var pathToRequirementsFile = Path.Combine(Environment.CurrentDirectory, "MultiObjectTracking", "requirements.txt");
        p.StartInfo.Arguments = $"-m pip install -r \"{pathToRequirementsFile}\"";
        p.Start();
    }
    
    public async Task SelectVideoFile()
    {
        var videoFilePicker = await _storageProviderService.GetStorageProvider().OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter = [AdditionalFilePickerFileTypes.Video],
            Title = "Open Video File"
        });

        if (!videoFilePicker.Any()) return;

        try
        {
            // TODO: load some metadata like FPS to select from
            var metadata = await VideoFileLoader.GetVideoMetadataAsync(videoFilePicker[0], string.Empty);
            VideoFilePath = metadata.VideoFilePath;
        }

        catch (Exception)
        {
            await MessageBox.ShowAsync("Unable to load selected file.",  "Error", MessageBoxIcon.Error);
        }
    }
    
    public async Task SelectOutputFolder()
    {
        var outputFolderPicker = await _storageProviderService.GetStorageProvider().OpenFolderPickerAsync(new ()
        {
            Title = "Select Output Folder"
        });

        if (!outputFolderPicker.Any()) return;
        
        OutputFolderPath = outputFolderPicker[0].Path.LocalPath[..^1];
    }

    public void StartProcess()
    {
        var outputPath = OutputFolderPath ?? throw new Exception("Output folder path cannot be empty");
        var pythonDir = Path.Combine(Environment.CurrentDirectory, "python_win64");
        var exe = Path.Combine(pythonDir, "python.exe");
        var script = Path.Combine(Environment.CurrentDirectory, "MultiObjectTracking", "MultiObjectTracking.py");
        
        var processStartInfo = new ProcessStartInfo
        {
            FileName = exe,
            Arguments = $"\"{script}\" --input_video=\"{VideoFilePath}\" --output_dir=\"{OutputFolderPath}\" --detector=\"{SelectedDetector}\" --weights=\"{SelectedModel}\" --tracker=\"{SelectedTracker}\"",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        var process = new Process { StartInfo = processStartInfo };
        
        process.OutputDataReceived += (_, args) =>
        {
            if (args.Data == null) return;
            
            _loggerService.LogMessage(args.Data);
            
            var initializingMatch = new Regex(@" Initializing .* engine...").Match(args.Data);
            
            if (initializingMatch.Success)
            {
                _progressBarService.StartProgress("Downloading model...");
                return;
            }

            var downloadingMatch = new Regex(@"Downloading.* (\d+)%").Match(args.Data);
            
            if (downloadingMatch.Success)
            {
                var currentDownloadPercent = int.Parse(downloadingMatch.Groups[1].Value);
                _progressBarService.SetProgress(currentDownloadPercent);
                return;
            }
            
            var detectorInfoMatch = new Regex(@" Detector:.*").Match(args.Data);
            
            if (detectorInfoMatch.Success)
            {
                _progressBarService.StartProgress("Processing Video...");
                return;
            }
            
            var videoProcessingProgressMatch = new Regex(@"Processing Video:.* (\d+)%").Match(args.Data);
            
            if (videoProcessingProgressMatch.Success)
            {
                var videoProcessingPercent = int.Parse(videoProcessingProgressMatch.Groups[1].Value);
                _progressBarService.SetProgress(videoProcessingPercent);
            }
        };
        
        // TODO: GPU ACCELERATION
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        Dispatcher.UIThread.Post(async void () =>
        {
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
                throw new Exception("Multi object tracking failed.");
        
            _progressBarService.FinishProgress("Finished: Multi Object Tracking");
            // TODO: get the path from the output of the script
            await _projectService.OpenDataset(Path.Combine(outputPath, $"frames/_{Path.GetFileNameWithoutExtension(VideoFilePath)}_metadata.xml"));
        });
        
        DialogCloseRequested?.Invoke();
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private record Detector(string Name, IEnumerable<string> Weights);
}
