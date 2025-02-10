using System;
using Python.Runtime;
using Snowman.DataContexts;

namespace Snowman.Core
{
    public class SnowmanApp
    {
        /// <summary>
        /// Data context for WorkingArea component containing data and methods that this component uses
        /// </summary>
        public WorkingAreaDataContext WorkingAreaDataContext { get; set; }
        
        public Project Project { get; set; }

        public SnowmanApp()
        {
            WorkingAreaDataContext = new WorkingAreaDataContext(this);
            Project = new(this);
            InitializePthonExecutionEnvironment();
        }

        private void InitializePthonExecutionEnvironment()
        {
            Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", "Python38/python38.dll");
            PythonEngine.Initialize();
            PythonEngine.BeginAllowThreads();
        }
    }
}
