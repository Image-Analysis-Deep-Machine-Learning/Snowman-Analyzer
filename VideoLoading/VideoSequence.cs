using System.Collections.Generic;
using Snowman.Data;

namespace Snowman.VideoLoading
{
    public class VideoSequence
    {
        public required List<Image> ImageList { get; set; }
        public required VideoSequenceMetadata Metadata { get; set; }
    }

    public class VideoSequenceMetadata
    {
        public double DurationSeconds => EndTime - StartTime;
        public double StartTime { get; set; }
        public double EndTime { get; set; }
        public double FrameRate { get; set; }
        public int FrameCount { get; set; }
        public required string FrameFolderPath { get; set; }
        public required string FrameFormat { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
