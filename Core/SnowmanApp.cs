using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Python.Runtime;
using Snowman.Controls;
using Snowman.Core.Scripting;
using Snowman.Core.Scripting.Nodes;
using Snowman.Core.Tools;
using Snowman.DataContexts;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.Core
{
    public class SnowmanApp
    {
        // it was 74 before
        public static SnowmanApp Instance => _instance;
        // it was 54 before
        public Project Project { get; }
        
        private const string ScriptsDirectory = "Scripts";
        public static SnowmanApp? _instance;
        
        public FrameTimelineDataContext FrameTimelineDataContext { get; }
        public EventTimelineDataContext EventTimelineDataContext { get; }
        public List<Script> Scripts  { get; } = [];

        private IServiceProvider ServiceProvider { get; set; } = null!;

        public SnowmanApp(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            FrameTimelineDataContext = new FrameTimelineDataContext();
            EventTimelineDataContext = new EventTimelineDataContext();
            Project = new Project(ServiceProvider); // TODO: this will need a factory that will rewire all existing services
            LoadScripts();
            InitializePythonExecutionEnvironment();
        }

        // TODO: dynamically load scripts from the directory while the app is running when the user opens combobox for script selection
        private void LoadScripts()
        {
            foreach (var file in Directory.EnumerateFiles(ScriptsDirectory))
            {
                Scripts.Add(new Script(file));
            }
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
        
        public ObjectsToRender? GetTempViewportVisuals()
        {
            if (Project is { TempEntities: not null, TempBoundingBoxes: not null })
            {
                return new ObjectsToRender
                {
                    CurrentImage = Project.CurrentFrame,
                    CurrentEntities = Project.TempEntities,
                    CurrentAnnotations = Project.TempBoundingBoxes
                };
            }
            
            return null;
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

        public async Task OpenProject(IStorageFile file)
        {
            await Project.OpenProject(file);
        }
    }
}
