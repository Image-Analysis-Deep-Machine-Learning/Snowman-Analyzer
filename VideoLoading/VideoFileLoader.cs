using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Snowman.Data;

namespace Snowman.VideoLoading
{
    public static class VideoFileLoader
    {
        // path to FFmpeg binary
        private static readonly string FfmpegPath = @"..\..\..\VideoLoading\ffmpeg.exe";
        // path to FFprobe binary
        private static readonly string FfprobePath = @"..\..\..\VideoLoading\ffprobe.exe";

        public static async Task<VideoFileSequence> ExtractFramesAsync(IStorageFile inputVideoFile, string outputFrameFolderPath, string frameFormat)
        {
            if (inputVideoFile == null)
                throw new FileNotFoundException("The input video file does not exist.");

            if (!Directory.Exists(outputFrameFolderPath))
                Directory.CreateDirectory(outputFrameFolderPath);
            
            var inputVideoFilePath = inputVideoFile.Path.LocalPath;

            // get video metadata
            var metadata = await GetVideoMetadata(inputVideoFilePath);

            var padding = metadata.FrameCount.ToString().Length;

            var framePattern = Path.Combine(outputFrameFolderPath, $"frame_%0{padding}d.{frameFormat}");
            
            // TODO: frame extraction works for file formats .mp4, .avi, .mov but not for .mkv
            var processStartInfo = new ProcessStartInfo
            {
                FileName = FfmpegPath,
                Arguments = $"-i \"{inputVideoFilePath}\" \"{framePattern}\"",
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = new Process { StartInfo = processStartInfo };

            // execute ffmpeg process asynchronously
            process.Start();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
                throw new Exception("FFmpeg failed to extract frames from the video.");

            ImageList imageList = new();
            
            var fileNames = Directory.GetFiles(outputFrameFolderPath, $"frame_*.{frameFormat}")
                .Select(Path.GetFileName)
                .ToArray();
            
            foreach (var fileName in fileNames)
            {
                if (fileName != null) imageList.Images.Add(new ImageFrame { Src = fileName });
            }

            var videoFileSequence = new VideoFileSequence
            {
                ImageList = imageList,
                Metadata = metadata,
                VideoFilePath = inputVideoFilePath,
                FrameFolderPath = outputFrameFolderPath,
                FrameFormat = frameFormat
            };

            return videoFileSequence;
        }

        private static async Task<VideoFileMetadata> GetVideoMetadata(string inputVideoFilePath)
        {
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

            return new VideoFileMetadata
            {
                VideoDurationSeconds = duration,
                Width = width,
                Height = height,
                FrameRate = frameRate
            };
        }
    }
}
