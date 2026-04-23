using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Snowman.Core.Services;
using Snowman.Data;
using Image = Snowman.Data.Image;

using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.Utilities;

public static class VideoFileLoader
{
    // TODO: support Linux and MaxOS ffmpeg binaries
    private const string FfmpegPath = @"ffmpeg\ffmpeg.exe";
    private const string FfprobePath = @"ffmpeg\ffprobe.exe";
    public static async Task<string> ExtractFramesAsync(VideoSequenceMetadata metadata, IServiceProvider serviceProvider)
    {
        var progressBarService = serviceProvider.GetService<IProgressBarService>();

        if (!Directory.Exists(metadata.OutputFolderPath))
            Directory.CreateDirectory(metadata.OutputFolderPath);

        var inputVideoFilePath = metadata.VideoFilePath;

        var padding = metadata.FrameCount.ToString().Length + 1;

        var framePattern = Path.Combine(metadata.OutputFolderPath, $"frame_%0{padding}d.{metadata.FrameFormat}");
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

        progressBarService.StartProgress("In progress: Loading video file");

        // send progress updates to the progress bar
        process.ErrorDataReceived += (_, args) =>
        {
            if (args.Data == null) return;

            var match = new Regex(@"frame=\s*(\d+)").Match(args.Data);
            if (!match.Success) return;

            var currentFrame = int.Parse(match.Groups[1].Value);
            var progress = Convert.ToInt32(Math.Round((double)currentFrame / metadata.FrameCount * 100));

            progressBarService.SetProgress(progress);
        };

        // execute ffmpeg process asynchronously
        process.Start();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
            throw new Exception("FFmpeg failed to extract frames from the video.");

        progressBarService.FinishProgress("Finished: Loading video file");

        List<Image> images = [];

        var fileNames = Directory.GetFiles(metadata.OutputFolderPath, $"frame_*.{metadata.FrameFormat}")
            .Select(Path.GetFileName)
            .ToArray();

        foreach (var fileName in fileNames)
        {
            if (fileName != null) images.Add(new Image { Src = fileName });
        }

        metadata.FrameCount = images.Count;

        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inputVideoFilePath);

        var newDataset = new DatasetData
        {
            Metadata = new Metadata
            {
                DataId = $"{fileNameWithoutExtension.ToLower()}_generated_dataset",
                VersionMajor = 1,
                Description = $"Generated dataset from {fileNameWithoutExtension}"
            },
            Images = images
        };
        
        var parsedText = DatasetData.Serialize(newDataset);
        var newDatasetXmlFilePath = Path.Combine(metadata.OutputFolderPath, $"{newDataset.Metadata.DataId}.xml");
        
        await File.WriteAllTextAsync(newDatasetXmlFilePath, parsedText);
        
        return newDatasetXmlFilePath;
    }

    public static async Task<VideoSequenceMetadata> GetVideoMetadataAsync(IStorageFile inputVideoFile,
        string outputFrameFolderPath)
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

        return new VideoSequenceMetadata
        {
            StartTime = 0,
            EndTime = duration,
            FrameRate = frameRate,
            FrameCount = Convert.ToInt32(Math.Round(frameRate * duration)),
            OutputFolderPath = outputFrameFolderPath,
            VideoFilePath = inputVideoFilePath,
            FrameFormat = "jpeg"
        };
    }
}

public class VideoSequenceMetadata
{
    public double DurationSeconds => EndTime - StartTime;
    public double StartTime { get; set; }
    public double EndTime { get; set; }
    public double FrameRate { get; set; }
    public int FrameCount { get; set; }
    public required string OutputFolderPath { get; set; }
    public required string VideoFilePath { get; set; }
    public required string FrameFormat { get; set; }
}
