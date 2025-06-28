using System;
using Python.Runtime;
using Snowman.Core.Tools;
using Snowman.DataContexts;

namespace Snowman.Core
{
    public class SnowmanApp
    {
        private static SnowmanApp? _instance;
        
        private Tool _activeTool = null!;
        
        public static SnowmanApp Instance => _instance ??= new SnowmanApp();
        public CanvasDataContext CanvasDataContext { get; }
        public FrameTimelineDataContext FrameTimelineDataContext { get; }
        public EventTimelineDataContext EventTimelineDataContext { get; }
        public Project Project { get; }

        public Tool ActiveTool
        {
            get => _activeTool;
            set
            {
                _activeTool = value;
                CanvasDataContext.ParentRendererControl.Cursor = value.Cursor;
            }
        }

        private SnowmanApp()
        {
            CanvasDataContext = new CanvasDataContext();
            FrameTimelineDataContext = new FrameTimelineDataContext();
            EventTimelineDataContext = new EventTimelineDataContext();
            Project = new Project();
            InitializePythonExecutionEnvironment();
        }

        public ObjectsToRender GetViewportVisuals()
        {
            return new ObjectsToRender
            {
                CurrentImage = Project.CurrentFrame,
                CurrentEntities = Project.Entities,
                CurrentAnnotations = Project.GetCurrentBoundingBoxes()
            };
        }

        private static void InitializePythonExecutionEnvironment()
        {
            if (Avalonia.Controls.Design.IsDesignMode) return; // do not initialize PythonEngine in the design mode to prevent crashes
            
            Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", "Python38/python38.dll"); // TODO: check env for Mac/Linux so it actually works
            PythonEngine.Initialize();
            PythonEngine.BeginAllowThreads();
        }
    }
}