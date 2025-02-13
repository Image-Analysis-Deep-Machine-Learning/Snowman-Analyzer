using System;
using Avalonia;
using Avalonia.Input;
using Python.Runtime;
using Snowman.Controls;
using Snowman.DataContexts;

namespace Snowman.Core
{
    public class SnowmanApp
    {
        
        private Tool _activeTool;
        /// <summary>
        /// Data context for WorkingArea component containing data and methods that this component uses
        /// </summary>
        public WorkingAreaDataContext WorkingAreaDataContext { get; set; }
        
        public TimelineDataContext TimelineDataContext { get; set; }
        
        public Project Project { get; set; }

        public Tool ActiveTool
        {
            get => _activeTool;
            set
            {
                _activeTool = value;
                WorkingAreaDataContext.RendererControl.Cursor = value.Cursor;
            }
        }

        public SnowmanApp(MainWindow mainWindow)
        {
            WorkingAreaDataContext = new WorkingAreaDataContext(this, mainWindow.WorkingAreaRenderer);
            TimelineDataContext = new TimelineDataContext(this);
            mainWindow.TimelineRenderer.RenderingContext = TimelineDataContext;
            Project = new Project(this);
            ActiveTool = Tool.MoveTool;
            InitializePythonExecutionEnvironment();
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