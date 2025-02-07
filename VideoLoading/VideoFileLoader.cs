using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Snowman.VideoLoading
{
    public class VideoFileLoader
    {
        // path to FFmpeg binary
        private const string FfmpegPath = @"../../../VideoLoading/ffmpeg.exe";

        // path to FFprobe binary
        private const string FfprobePath = @"../../../VideoLoading/ffprobe.exe";

        public static async Task<VideoFileSequence> ExtractFramesAsync(
            string inputVideoFilePath,
            string outputFrameFolderPath,
            string frameFormat,
            TimeSpan? startTime = null,
            TimeSpan? endTime = null,
            double? targetFrameRate = null)
        {
            if (!File.Exists(inputVideoFilePath))
                throw new FileNotFoundException("The input video file does not exist.");

            if (!Directory.Exists(outputFrameFolderPath))
                Directory.CreateDirectory(outputFrameFolderPath);

            // get video metadata
            var metadata = GetVideoMetadata(inputVideoFilePath);

            // duration of the specified time range
            var startTimeSeconds = startTime?.TotalSeconds ?? 0;
            var endTimeSeconds = endTime?.TotalSeconds ?? metadata.VideoDurationSeconds;
            var rangeDuration = Math.Max(0, endTimeSeconds - startTimeSeconds);
            
            var frameRate = targetFrameRate ?? metadata.FrameRate;
            
            // frame count based on the desired FPS and specified time range
            var rangeFrameCount = (int)(frameRate * rangeDuration);
            
            var arguments = new StringBuilder();
    
            if (startTime.HasValue)
                arguments.Append($"-ss {startTime.Value} ");
    
            // input video file
            arguments.Append($"-i \"{inputVideoFilePath}\" ");
            
            if (targetFrameRate.HasValue)
                arguments.Append($"-vf fps={targetFrameRate.Value} ");
    
            if (endTime.HasValue)
                arguments.Append($"-to {endTime.Value} ");
            
            var padding = rangeFrameCount.ToString().Length;
            var framePattern = Path.Combine(outputFrameFolderPath, $"frame_%0{padding}d.{frameFormat}");
    
            // output frame
            arguments.Append($"\"{framePattern}\"");
            
            var processStartInfo = new ProcessStartInfo()
            {
                FileName = FfmpegPath,
                Arguments = arguments.ToString(),
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = new Process { StartInfo = processStartInfo };

            // execute ffmpeg process asynchronously
            await Task.Run(() => process.Start());
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
                throw new Exception("FFmpeg failed to extract frames from the video.");

            // make a list of all frame paths
            List<string> framePaths = [.. Directory.GetFiles(outputFrameFolderPath, $"frame_*.{frameFormat}")];

            var videoFileSequence = new VideoFileSequence
            {
                FramePaths = framePaths,
                Metadata = metadata,
                VideoFilePath = inputVideoFilePath,
                FrameFolderPath = outputFrameFolderPath,
                FrameFormat = frameFormat
            };

            return videoFileSequence;
        }

        private static VideoFileMetadata GetVideoMetadata(string inputVideoFilePath)
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

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new Exception("FFprobe failed to retrieve metadata.");

            // parse metadate from json
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
