using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Snowman.Controls;
using Snowman.Core;
using Snowman.Core.Services;
using Snowman.Core.Tools;
using Snowman.Data;
using Snowman.Factories;
using Snowman.Utilities;
using Ursa.Controls;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.DataContexts;

public class MainWindowDataContext
{
    private string? _lastCustomZoom;
    
    public bool IsEntitySelected => SnowmanApp.Instance.Project.SelectedEntity is not null;
    public IServiceProvider ServiceProvider { get; set; }
    
    public MainWindowDataContext()
    {
        ServiceProvider = new SnowmanServiceProvider();
    }
    
    public void NewProject()
    {
            
    }
    
    public async Task OpenProject()
    {
        var filePickerResult = await StorageProviderFactory.GetStorageProvider().OpenFilePickerAsync(new FilePickerOpenOptions
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
            return;
        }
        // TODO: commented due to inaccessibility of controls from datacontext I FUCKING LOVE MVC AND XAML UI FRAMEWORK HAHAHAHAHHAHAHAHAHAHHAHAHAHA
        /*RendererControl.InvalidateVisual();
        FrameTimeline.InvalidateVisual();
        EventTimeline.InvalidateVisual();*/
    }
    
    public async Task SaveProject()
    {
        var filePickerResult = await StorageProviderFactory.GetStorageProvider().SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            FileTypeChoices = [AdditionalFilePickerFileTypes.Xml],
            Title = "Save Project File"
        });

        if (filePickerResult is null) return;

        try
        {
            await SnowmanApp.Instance.Project.SaveProject(filePickerResult);
        }

        catch (Exception ex)
        {
            await MessageBox.ShowAsync("Unable to load selected file.",  "Error", MessageBoxIcon.Error);
            return;
        }
        // TODO: commented due to inaccessibility of controls from datacontext
        /*RendererControl.InvalidateVisual();
        FrameTimeline.InvalidateVisual();
        EventTimeline.InvalidateVisual();*/
    }
    
    public async Task OpenXml()
    {
        var filePickerResult = await StorageProviderFactory.GetStorageProvider().OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter = [AdditionalFilePickerFileTypes.Xml],
            Title = "Open XML File"
        });

        if (!filePickerResult.Any()) return;

        try
        {
            await SnowmanApp.Instance.Project.OpenXml(filePickerResult[0]);
        }

        catch (Exception)
        {
            await MessageBox.ShowAsync("Unable to load selected file.",  "Error", MessageBoxIcon.Error);
            return;
        }
        // TODO: commented due to inaccessibility of controls from datacontext
        /*RendererControl.InvalidateVisual();
        FrameTimeline.InvalidateVisual();
        EventTimeline.InvalidateVisual();*/
    }
    
    public async Task LoadVideoFile()
    {
        var filePickerResult = await StorageProviderFactory.GetStorageProvider().OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter = [AdditionalFilePickerFileTypes.Video],
            Title = "Open Video File"
        });

        if (!filePickerResult.Any()) return;

        var ownerWindow = this;
        // HOW JUST HOW
        //await SnowmanApp.Instance.Project.LoadVideoFile(filePickerResult[0], ownerWindow, ProgressBar, ProgressBarText);
        // TODO: commented due to inaccessibility of controls from datacontext
        /*RendererControl.InvalidateVisual();
        FrameTimeline.InvalidateVisual();
        EventTimeline.InvalidateVisual();*/
    }
    
    public void PrevFrame()
    {
        SnowmanApp.Instance.Project.PreviousFrame();
        //I will need to do these with events as well I guess FFS
        /*RendererControl.InvalidateVisual();
        FrameTimeline.InvalidateVisual();*/
    }
        
    public void NextFrame()
    {
        SnowmanApp.Instance.Project.NextFrame();
        /*RendererControl.InvalidateVisual();
        FrameTimeline.InvalidateVisual();*/
    }

    public void UpdateFrame()
    {
        /*RendererControl.InvalidateVisual();
        FrameTimeline.InvalidateVisual();*/
    }
    
    public ObservableCollection<string> ZoomScaleOptions { get; } = ["1x", "2x", "5x", "10x", "20x"];
    private static string FormatZoomScale(double value) => $"{value:0.#}x";
    public string ZoomScaleString
    {
        get => FormatZoomScale(SnowmanApp.Instance.EventTimelineDataContext.ZoomScale);
        set
        {
            if (value.EndsWith('x') && double.TryParse(value.TrimEnd('x'), out var parsed))
            {
                SnowmanApp.Instance.EventTimelineDataContext.ZoomScale = parsed;
                // EVEEEEEENTS SDASOWFJIASNFLKJNASFNLASJKNFSJLABNFJLKSABFJKLasd
                /*OnPropertyChanged();
                EventTimeline.InvalidateVisual();*/
            }
        }
    }

    public IEnumerable<Tool> Tools => ToolRegistry.GetTools(ServiceProvider);

    public void SetupZoomScaleChangedHandler()
    {
        SnowmanApp.Instance.EventTimelineDataContext.ZoomScaleChanged += () =>
        {
            var zoomScale = FormatZoomScale(SnowmanApp.Instance.EventTimelineDataContext.ZoomScale);

            if (!ZoomScaleOptions.Contains(zoomScale))
            {
                if (_lastCustomZoom is not null) ZoomScaleOptions.Remove(_lastCustomZoom);
                ZoomScaleOptions.Add(zoomScale);
                _lastCustomZoom = zoomScale;
            }
            else if (_lastCustomZoom != null && zoomScale != _lastCustomZoom)
            {
                var toRemove = _lastCustomZoom;
                _lastCustomZoom = null;
                // remove the temporary zoom scale after UI update finishes
                Dispatcher.UIThread.Post(() =>
                {
                    ZoomScaleOptions.Remove(toRemove);
                }, Avalonia.Threading.DispatcherPriority.Background);
            }
            //you know what
            //OnPropertyChanged(nameof(ZoomScaleString));
        };
    }
    
    
    public void Demo()
    {
        if (Design.IsDesignMode) return;

        var ruleId = SnowmanApp.Instance.Project.Rules.Count;
        var ruleName = new StringBuilder();

        /*if (IsEntitySelected)
            for (var i = 0; i < SnowmanApp.Instance.Project.SelectedEntity!.Scripts.Count; i++)
            {
                var script = SnowmanApp.Instance.Project.SelectedEntity.Scripts[i];
                ruleName.Append(script.Name);
                if (i != SnowmanApp.Instance.Project.SelectedEntity.Scripts.Count - 1) ruleName.Append(" + ");
            }
*/
        var (baseColor, lightColor) = ColorGeneration.GetHuePair(ruleId);
        EventTimelineDataContext.TimelineColors.TryAdd(ruleId, (baseColor, lightColor));
            
        var output = SnowmanApp.Instance.Project.Demo();
        //DemoOutput.Text = output.Item1; // TODO: bindings

        SnowmanApp.Instance.Project.Rules.Add(new RuleData(ruleId, ruleName.ToString(), output.Item3));
        SnowmanApp.Instance.Project.EventsByFrameIndexByRuleId.Add(ruleId, output.Item2 ?? new Dictionary<int, List<EventData>>());
            
        SnowmanApp.Instance.EventTimelineDataContext.Redraw();
        //EventTimeline.InvalidateVisual();
    }

    public void ClearEventInfo()
    {
        SnowmanApp.Instance.Project.TempEntities = null;
        SnowmanApp.Instance.Project.TempBoundingBoxes = null;
            
        //SnowmanApp.Instance.RendererDataContext.ParentRendererControl.InvalidateVisual();
        SnowmanApp.Instance.FrameTimelineDataContext.ParentRendererControl.InvalidateVisual();
            
        //InfoBox.Text = string.Empty; // bindings TODO
    }
}
