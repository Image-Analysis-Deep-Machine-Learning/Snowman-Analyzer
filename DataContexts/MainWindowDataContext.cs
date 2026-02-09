using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Snowman.Core;
using Snowman.Core.Services;
using Snowman.Utilities;
using Ursa.Controls;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.DataContexts;

public partial class MainWindowDataContext : INotifyPropertyChanged
{
    private readonly IDatasetImagesService _datasetImagesService;
    private readonly IStorageProviderService _storageProviderService;
    
    public event PropertyChangedEventHandler? PropertyChanged;

    public bool FrameTimelineActive => ActiveTimeline == TimelineMode.Frame;
    public bool EventTimelineActive => ActiveTimeline == TimelineMode.Event;
    
    private TimelineMode ActiveTimeline
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(FrameTimelineActive));
            OnPropertyChanged(nameof(EventTimelineActive));
        }
    }

    public MainWindowDataContext(IServiceProvider serviceProvider)
    {
        _datasetImagesService = serviceProvider.GetService<IDatasetImagesService>();
        _storageProviderService = serviceProvider.GetService<IStorageProviderService>();
    }

    public void SetTimelineMode(TimelineMode mode)
    {
        ActiveTimeline = mode;
    }
    
    public void NewProject() { } // TODO
    
    public async Task OpenProject()
    {
        var filePickerResult = await _storageProviderService.GetStorageProvider().OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter = [AdditionalFilePickerFileTypes.Xml],
            Title = "Open Project File"
        });

        if (!filePickerResult.Any()) return;

        try
        {
            await SnowmanApp.Instance.OpenProject(filePickerResult[0]);
        }

        catch (Exception)
        {
            await MessageBox.ShowAsync("Unable to load selected file.",  "Error", MessageBoxIcon.Error);
        }
    }
    
    public async Task SaveProject()
    {
        var filePickerResult = await _storageProviderService.GetStorageProvider().SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            FileTypeChoices = [AdditionalFilePickerFileTypes.Xml],
            Title = "Save Project File"
        });

        if (filePickerResult is null) return;

        try
        {
            await SnowmanApp.Instance.Project.SaveProject(filePickerResult);
        }

        catch (Exception)
        {
            await MessageBox.ShowAsync("Unable to load selected file.",  "Error", MessageBoxIcon.Error);
        }
    }
    
    public async Task OpenDataset()
    {
        var filePickerResult = await _storageProviderService.GetStorageProvider().OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter = [AdditionalFilePickerFileTypes.Xml],
            Title = "Open Dataset XML File"
        });

        if (!filePickerResult.Any()) return;

        try
        {
            await SnowmanApp.Instance.Project.OpenDataset(filePickerResult[0]);
        }

        catch (Exception)
        {
            await MessageBox.ShowAsync("Unable to load selected file.",  "Error", MessageBoxIcon.Error);
        }
    }
    
    public async Task LoadVideoFile()
    {
        var filePickerResult = await _storageProviderService.GetStorageProvider().OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter = [AdditionalFilePickerFileTypes.Video],
            Title = "Open Video File"
        });

        if (!filePickerResult.Any()) return;

        var ownerWindow = this;
        // HOW JUST HOW
        //await SnowmanApp.Instance.Project.LoadVideoFile(filePickerResult[0], ownerWindow, ProgressBar, ProgressBarText);
    }

    public void PrevFrame()
    {
        _datasetImagesService.PrevFrame();
    }

    public void NextFrame()
    {
        _datasetImagesService.NextFrame();
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
    
    public enum TimelineMode { Frame, Event }
}
