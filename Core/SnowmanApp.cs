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
        public WorkingAreaDataContext WorkingAreaDataContext { get; set; }
        public TimelineDataContext TimelineDataContext { get; set; }
        public Project Project { get; set; }

        public Tool ActiveTool
        {
            get => _activeTool;
            set
            {
                _activeTool = value;
                WorkingAreaDataContext.Control.Cursor = value.Cursor;
            }
        }

        private SnowmanApp()
        {
            WorkingAreaDataContext = new WorkingAreaDataContext();
            TimelineDataContext = new TimelineDataContext();
            Project = new Project();
            InitializePythonExecutionEnvironment();
        }

        public ViewportVisuals GetViewportVisuals()
        {
            return new ViewportVisuals
            {
                CurrentImage = Project.CurrentFrame,
                CurrentEntities = Project.Entities,
                CurrentAnnotations = Project.GetCurrentBoundingBoxes()
            };
        }

        private static void InitializePythonExecutionEnvironment()
        {
            if (Avalonia.Controls.Design.IsDesignMode) return; // do not initialize PythonEngine in the design mode to prevent crashes
            
            Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", "Python38/python38.dll");
            PythonEngine.Initialize();
            PythonEngine.BeginAllowThreads();
        }
    }
}