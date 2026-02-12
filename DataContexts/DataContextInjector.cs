using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Snowman.Controls;
using Snowman.Core.Services;
using Snowman.Core.Services.Impl;
using Snowman.Events.Suppliers;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.DataContexts;

public static class DataContextInjector
{
    private static readonly Dictionary<Type, Func<Control, IServiceProvider, object>> FactoryDict = [];
    
    static DataContextInjector()
    {
        RegisterDataContextFactories();
    }

    public static void RegisterDataContextFactory<TControl>(Func<TControl, IServiceProvider, object> factory) where TControl : Control
    {
        FactoryDict[typeof(TControl)] = (control, serviceProvider) => factory((control as TControl)!, serviceProvider);
    }

    public static void TryInjectDataContext(Control control, IServiceProvider? provider)
    {
        if (provider is null) return;
        
        var dataContext = GetDataContextFor(control, provider);
        
        if (dataContext is not null)
        {
            control.DataContext = dataContext;
        }
    }

    private static object? GetDataContextFor(Control control, IServiceProvider provider)
    {
        return FactoryDict.TryGetValue(control.GetType(), out var factory) ? factory(control, provider) : null;
    }

    // keep in alphabetic order to make it easier to read
    private static void RegisterDataContextFactories()
    {
        RegisterDataContextFactory<EventTimeline>((control, serviceProvider) =>
        {
            var dataContext =  new EventTimelineDataContext(serviceProvider, control.Canvas, control.TimelineOutput, control.Bounds);
            dataContext.UpdateEventPins(dataContext.Zoom, 0);
            return dataContext;
        });
        RegisterDataContextFactory<EventTimelineViewport>((control, serviceProvider) =>
        {
            var newDataContext = new EventTimelineViewportDataContext(serviceProvider, control.TimelineViewer);
            serviceProvider.RegisterService<ITimelineService>(new TimelineServiceImpl(newDataContext));
            return newDataContext;
        });
        RegisterDataContextFactory<FrameTimeline>((_, serviceProvider) => new FrameTimelineDataContext(serviceProvider));
        RegisterDataContextFactory<GraphOverlay>((_, serviceProvider) => new GraphOverlayDataContext(serviceProvider));
        // NodeControl is dynamically generated, it does not use this pattern TODO: check if it can be done
        RegisterDataContextFactory<NodePort>((control, serviceProvider) =>
        {
            var nodeService = serviceProvider.GetService<INodeService>();
            nodeService.RegisterNodePort(control);
            return new NodePortDataContext(serviceProvider);
        });
        RegisterDataContextFactory<NodeViewport>((control, serviceProvider) =>
        {
            serviceProvider.GetService<IEventManager>().RegisterEventSupplier<INodeViewportEventSupplier>(control);
            var nodeService = new NodeServiceImpl(control.ViewportCanvas, control.BackgroundOverlay, control.ForegroundOverlay, serviceProvider);
            serviceProvider.RegisterService<INodeService>(nodeService);
            return new NodeViewportDataContext(serviceProvider);
        });
        RegisterDataContextFactory<ToolBar>((_, serviceProvider) => new ToolBarDataContext(serviceProvider));
        RegisterDataContextFactory<Viewport>((control, serviceProvider) =>
        {
            serviceProvider.GetService<IEventManager>().RegisterActionOnSupplier<IDatasetImagesEventSupplier>(x => x.SelectedFrameChanged += control.InvalidateVisual);
            serviceProvider.GetService<IEventManager>().RegisterActionOnSupplier<IProjectEventSupplier>(x => x.DatasetLoaded += control.InvalidateVisual);
            return new ViewportDataContext(serviceProvider);
        });
        RegisterDataContextFactory<ViewportWindow>((_, _) => new ViewportWindowDataContext());
    }
}
