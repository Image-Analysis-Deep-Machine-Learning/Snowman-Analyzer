using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Snowman.VideoLoading
{
    public class VideoFileLoader
    {
        // path to FFmpeg binary
        private static readonly string ffmpegPath = @"../../../VideoLoading/ffmpeg.exe";
        // path to FFprobe binary
        private static readonly string ffprobePath = @"../../../VideoLoading/ffprobe.exe";

        public static async Task<VideoFileSequence> ExtractFramesAsync(string inputVideoFilePath, string outputFrameFolderPath, string frameFormat)
        {
            if (!File.Exists(inputVideoFilePath))
                throw new FileNotFoundException("The input video file does not exist.");

            if (!Directory.Exists(outputFrameFolderPath))
                Directory.CreateDirectory(outputFrameFolderPath);

            // get video metadata
            VideoFileMetadata metadata = GetVideoMetadata(inputVideoFilePath);

            int padding = metadata.FrameCount.ToString().Length;

            string framePattern = Path.Combine(outputFrameFolderPath, $"frame_%0{padding}d.{frameFormat}");
            var processStartInfo = new ProcessStartInfo()
            {
                FileName = ffmpegPath,
                Arguments = $"-i \"{inputVideoFilePath}\" \"{framePattern}\"",
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
                FileName = ffprobePath,
                Arguments = $"-v error -select_streams v:0 -show_entries stream=width,height,r_frame_rate,duration -of json \"{inputVideoFilePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // execute ffprobe process
            using var process = new Process { StartInfo = processStartInfo };
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new Exception("FFprobe failed to retrieve metadata.");

            // parse metadate from json
            var json = JsonDocument.Parse(output);
            var stream = json.RootElement.GetProperty("streams")[0];

            string? rFrameRate = stream.GetProperty("r_frame_rate").GetString() ?? throw new Exception("Failed to retrieve frame rate.");
            string[] frameRateParts = rFrameRate.Split('/');
            double frameRate = double.Parse(frameRateParts[0]) / double.Parse(frameRateParts[1]);

            string? durationStr = stream.GetProperty("duration").GetString() ?? throw new Exception("Failed to retrieve duration.");
            double duration = double.Parse(durationStr, CultureInfo.InvariantCulture);

            int width = stream.GetProperty("width").GetInt32();
            int height = stream.GetProperty("height").GetInt32();

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
