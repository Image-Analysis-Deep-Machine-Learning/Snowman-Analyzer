using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Snowman.Core.Services;
using Snowman.Core.Settings;
using Snowman.Events.Suppliers;
using Snowman.Utilities;
using Ursa.Controls;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.DataContexts;

public partial class MainWindowDataContext : INotifyPropertyChanged
{
    private readonly IDatasetImagesService _datasetImagesService;
    private readonly IStorageProviderService _storageProviderService;
    private readonly IProjectService _projectService;
    
    public event PropertyChangedEventHandler? PropertyChanged;

    public bool FrameTimelineActive => ActiveTimeline == TimelineMode.Frame;
    public bool EventTimelineActive => ActiveTimeline == TimelineMode.Event;

    public string CurrentFrame =>
        $"Frame {_datasetImagesService.CurrentFrameIndex() + 1}";

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
        serviceProvider.GetService<IEventManager>().RegisterActionOnSupplier<IDatasetImagesEventSupplier>(supplier =>
        {
            supplier.SelectedFrameChanged += () => { OnPropertyChanged(nameof(CurrentFrame)); };
        });
        _storageProviderService = serviceProvider.GetService<IStorageProviderService>();
        _projectService = serviceProvider.GetService<IProjectService>();
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
            await _projectService.OpenProject(filePickerResult[0]);
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
            await _projectService.SaveProject(filePickerResult);
        }

        catch (Exception e)
        {
            await MessageBox.ShowAsync($"Unable to load selected file.\nError:\n{e}",  "Error", MessageBoxIcon.Error);
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
            await _projectService.OpenDataset(filePickerResult[0].Path.LocalPath);
        }

        catch (Exception)
        {
            await MessageBox.ShowAsync("Unable to load selected file.",  "Error", MessageBoxIcon.Error);
        }
    }
    
    public void OpenSettings()
    {
        SettingsRegistry.OpenSettingsWindow();
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
