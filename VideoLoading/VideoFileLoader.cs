using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Snowman.Data;

namespace Snowman.VideoLoading
{
    public static partial class VideoFileLoader
    {
        // path to FFmpeg binary
        private const string FfmpegPath = @"..\..\..\VideoLoading\ffmpeg.exe";

        // path to FFprobe binary
        private const string FfprobePath = @"..\..\..\VideoLoading\ffprobe.exe";

        public static async Task<VideoSequence> ExtractFramesAsync(IStorageFile inputVideoFile, VideoSequenceMetadata metadata, ProgressBar progressBar)
        {
            if (inputVideoFile == null)
                throw new FileNotFoundException("The input video file does not exist.");

            if (!Directory.Exists(metadata.FrameFolderPath))
                Directory.CreateDirectory(metadata.FrameFolderPath);
            
            var inputVideoFilePath = inputVideoFile.Path.LocalPath;

            var padding = metadata.FrameCount.ToString().Length;

            var framePattern = Path.Combine(metadata.FrameFolderPath, $"frame_%0{padding}d.{metadata.FrameFormat}");
            var startTime = TimeSpan.FromSeconds(metadata.StartTime).ToString(@"hh\:mm\:ss\.fff");
            var endTime = TimeSpan.FromSeconds(metadata.EndTime).ToString(@"hh\:mm\:ss\.fff");
            var targetFps = metadata.FrameRate.ToString(CultureInfo.InvariantCulture);
            
            // TODO: fix: frame extraction doesn't work for input video file format .mkv (works fine for .mp4, .mov, .avi)
            // TODO: fix: frame extraction doesn't work for output frame file format .gif (works fine for .jpeg, .png, .tiff, .bmp)
            var processStartInfo = new ProcessStartInfo
            {
                FileName = FfmpegPath,
                Arguments = $"-ss {startTime} -to {endTime} -i \"{inputVideoFilePath}\" -r {targetFps} \"{framePattern}\"",
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = new Process { StartInfo = processStartInfo };
            
            Dispatcher.UIThread.Post(() => progressBar.Value = 0);
            Dispatcher.UIThread.Post(() => progressBar.IsVisible = true);
            
            // send progress updates to the progress bar
            process.ErrorDataReceived += (_, args) =>
            {
                if (args.Data == null) return;
                
                var match = MyRegex().Match(args.Data);
                if (!match.Success) return;
                
                var currentFrame = int.Parse(match.Groups[1].Value);
                var progress = Convert.ToInt32(Math.Round((double)currentFrame / metadata.FrameCount * 100));
                
                Dispatcher.UIThread.Post(() => progressBar.Value = progress);
            };

            // execute ffmpeg process asynchronously
            process.Start();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
                throw new Exception("FFmpeg failed to extract frames from the video.");

            Dispatcher.UIThread.Post(() => progressBar.IsVisible = false);
            
            ImageList imageList = new();
            
            var fileNames = Directory.GetFiles(metadata.FrameFolderPath, $"frame_*.{metadata.FrameFormat}")
                .Select(Path.GetFileName)
                .ToArray();
            
            foreach (var fileName in fileNames)
            {
                if (fileName != null) imageList.Images.Add(new ImageFrame { Src = fileName });
            }
            
            metadata.FrameCount = imageList.Images.Count;

            var videoFileSequence = new VideoSequence
            {
                ImageList = imageList,
                Metadata = metadata
            };

            return videoFileSequence;
        }
        
        [GeneratedRegex(@"frame=\s*(\d+)")]
        private static partial Regex MyRegex();

        public static async Task<VideoSequenceMetadata> GetVideoMetadataAsync(IStorageFile inputVideoFile, string outputFrameFolderPath)
        {
            if (inputVideoFile == null)
                throw new FileNotFoundException("The input video file does not exist.");
            
            var inputVideoFilePath = inputVideoFile.Path.LocalPath;
            
            var processStartInfo = new ProcessStartInfo
            {
                FileName = FfprobePath,
                Arguments = $"-v error -select_streams v:0 -show_entries stream=width,height,r_frame_rate,duration -of json \"{inputVideoFilePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // execute ffprobe process
            using var process = new Process();
            process.StartInfo = processStartInfo;
            
            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
                throw new Exception("FFprobe failed to retrieve metadata.");

            // parse metadata from json
            var json = JsonDocument.Parse(output);
            var stream = json.RootElement.GetProperty("streams")[0];

            var rFrameRate = stream.GetProperty("r_frame_rate").GetString() ?? throw new Exception("Failed to retrieve frame rate.");
            var frameRateParts = rFrameRate.Split('/');
            var frameRate = double.Parse(frameRateParts[0]) / double.Parse(frameRateParts[1]);
            
            var durationStr = stream.GetProperty("duration").GetString() ?? throw new Exception("Failed to retrieve duration.");
            var duration = double.Parse(durationStr, CultureInfo.InvariantCulture);

            var width = stream.GetProperty("width").GetInt32();
            var height = stream.GetProperty("height").GetInt32();

            return new VideoSequenceMetadata
            {
                StartTime = 0,
                EndTime = duration,
                FrameRate = frameRate,
                FrameCount = Convert.ToInt32(Math.Ceiling(frameRate * duration)),
                FrameFolderPath = outputFrameFolderPath,
                FrameFormat = "jpeg",
                Width = width,
                Height = height
            };
        }
    }
}
