using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Snowman.Core.Services;
using Snowman.Utilities;
using Ursa.Controls;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.DataContexts;

public partial class LoadVideoWindowDataContext : INotifyPropertyChanged
{
    private readonly IStorageProviderService _storageProviderService;
    private readonly IMessageBoxService _messageBoxService;
    private VideoSequenceMetadata? _videoMetadata;
    
    public event PropertyChangedEventHandler? PropertyChanged;
    public event Events.EventHandler<VideoSequenceMetadata?>? DialogCloseRequested;
    public double SelectedFrameRate
    {
        get => _videoMetadata?.FrameRate ?? 1;
        set
        {
            _videoMetadata?.FrameRate = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedFrameRateStr));
        }
    }

    public double StartSelectedTime
    {
        get => _videoMetadata?.StartTime ?? 0;
        set
        {
            _videoMetadata?.StartTime = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(LowerSelectedTimeStr));
        }
    }

    public double EndSelectedTime
    {
        get => _videoMetadata?.EndTime ?? 0;
        set
        {
            _videoMetadata?.EndTime = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(UpperSelectedTimeStr));
        }
    }

    public string SelectedFrameFormat
    {
        get => _videoMetadata?.FrameFormat ?? "jpeg";
        set
        {
            _videoMetadata?.FrameFormat = value;
            OnPropertyChanged();
        }
    }

    public ReadOnlyCollection<string> FrameFormats { get; } = new(["jpeg", "png", "gif", "tiff", "bmp"]);
    public double DurationSeconds => _videoMetadata?.DurationSeconds ?? 1;
    public double FrameRate => _videoMetadata?.FrameRate ?? 1;
    public string LowerSelectedTimeStr => $@"{TimeSpan.FromSeconds(StartSelectedTime):mm\:ss\.fff}";
    public string UpperSelectedTimeStr => $@"{TimeSpan.FromSeconds(EndSelectedTime):mm\:ss\.fff}";
    public string SelectedFrameRateStr => $"{SelectedFrameRate:0.###} FPS";
    public bool MetadataSet => _videoMetadata is not null;

    public LoadVideoWindowDataContext(IServiceProvider serviceProvider)
    {
        _storageProviderService = serviceProvider.GetService<IStorageProviderService>();
        _messageBoxService = serviceProvider.GetService<IMessageBoxService>();
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
        
        var outputFolderPicker = await _storageProviderService.GetStorageProvider().OpenFolderPickerAsync(new ()
        {
            
            Title = "Select Output Folder"
        });

        if (!outputFolderPicker.Any()) return;

        try
        {
            _videoMetadata = await VideoFileLoader.GetVideoMetadataAsync(videoFilePicker[0], outputFolderPicker[0].Path.LocalPath);
            OnPropertyChanged(nameof(DurationSeconds));
            OnPropertyChanged(nameof(FrameRate));
            OnPropertyChanged(nameof(SelectedFrameRate));
            OnPropertyChanged(nameof(StartSelectedTime));
            OnPropertyChanged(nameof(EndSelectedTime));
            OnPropertyChanged(nameof(SelectedFrameFormat));
            OnPropertyChanged(nameof(LowerSelectedTimeStr));
            OnPropertyChanged(nameof(UpperSelectedTimeStr));
            OnPropertyChanged(nameof(SelectedFrameRateStr));
            OnPropertyChanged(nameof(MetadataSet));
        }

        catch (Exception)
        {
            _messageBoxService.ShowMessageBox("Error", "Unable to load selected file.", MessageBoxIcon.Error);
        }
    }
    
    public void Close()
    {
        DialogCloseRequested?.Invoke(null);
    }

    public void Submit()
    {
        DialogCloseRequested?.Invoke(_videoMetadata);
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
}
