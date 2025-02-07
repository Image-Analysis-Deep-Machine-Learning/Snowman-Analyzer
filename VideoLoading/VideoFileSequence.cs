using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snowman.Data;

namespace Snowman.VideoLoading
{
    public class VideoFileSequence
    {
        public ImageList? ImageList { get; set; }
        public VideoFileMetadata? Metadata { get; set; }
        public string? VideoFilePath { get; set; }
        public string? FrameFolderPath { get; set; }
        public string? FrameFormat { get; set; }
    }

    public class VideoFileMetadata
    {
        public double VideoDurationSeconds { get; init; }
        public double FrameRate { get; init; }
        public int FrameCount => (int)(FrameRate * VideoDurationSeconds);
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
