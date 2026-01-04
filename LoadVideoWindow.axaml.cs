using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Snowman.VideoLoading;

namespace Snowman
{
    public partial class LoadVideoWindow : Window, INotifyPropertyChanged
    {
        private double _startSelectedTime;
        private double _endSelectedTime;
        public new event PropertyChangedEventHandler? PropertyChanged;
        private double _selectedFps;
        private string _selectedFrameFormat = "jpeg";

        public LoadVideoWindow()
        {
            
        }
        
        public LoadVideoWindow(VideoSequenceMetadata metadata)
        {
            InitializeComponent();
            DurationSeconds = metadata.DurationSeconds;
            FrameRate = metadata.FrameRate;
            DataContext = this;
            StartSelectedTime = metadata.StartTime;
            EndSelectedTime = metadata.EndTime;
            SelectedFps = metadata.FrameRate;
        }
        
        public double DurationSeconds { get; }
        
        public double FrameRate { get; }

        public double SelectedFps
        {
            get => _selectedFps;
            set
            {
                _selectedFps = value;
                OnPropertyChanged(nameof(SelectedFps));
                OnPropertyChanged(nameof(SelectedFpsStr));
            }
        }

        public double StartSelectedTime
        {
            get => _startSelectedTime;
            set
            {
                _startSelectedTime = value;
                OnPropertyChanged(nameof(StartSelectedTime));
                OnPropertyChanged(nameof(LowerSelectedTimeStr));
            }
        }

        public double EndSelectedTime
        {
            get => _endSelectedTime;
            set
            {
                _endSelectedTime = value;
                OnPropertyChanged(nameof(EndSelectedTime));
                OnPropertyChanged(nameof(UpperSelectedTimeStr));
            }
        }
        
        public string LowerSelectedTimeStr => $@"{TimeSpan.FromSeconds(StartSelectedTime):mm\:ss\.fff}";
        
        public string UpperSelectedTimeStr => $@"{TimeSpan.FromSeconds(EndSelectedTime):mm\:ss\.fff}";
        
        public string SelectedFpsStr => $"{SelectedFps:0.###} FPS";

        public List<string> FrameFormats { get; } = ["jpeg", "png", "gif", "tiff", "bmp"];
        
        public string SelectedFrameFormat
        {
            get => _selectedFrameFormat;
            set
            {
                _selectedFrameFormat = value;
                OnPropertyChanged(nameof(SelectedFrameFormat));
            }
        }
        
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SubmitButton_OnClick(object? sender, RoutedEventArgs e)
        {
            Close(true);
        }

        private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}