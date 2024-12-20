using System.Diagnostics;
using YoutubeExplode;

namespace KIBAEMON2024_CSharp.Service;

/// <summary>
/// 유튜브 오디오 스트림을 제공하는 클래스
/// 절차:
/// 1. yt-dlp로 오디오 스트림을 받아 ffmpeg으로 파이핑
/// 2. ffmpeg으로 파이핑된 오디오 스트림을 Discord에 전달
/// </summary>
public class YoutubeStreamProvider : IStreamProvider
{
    protected Process? YtdLp { get; set; } = null;
    protected Process? FFmpeg { get; set; } = null;
    protected Task? PipeTask { get; set; } = null;

    public bool IsMine(string url)
    {
        return url.Contains("youtube.com") || url.Contains("youtu.be");
    }

    public Process StartStream(string url)
    {
        YtdLp = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                Arguments = $"-f bestaudio -o - \"{url}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        FFmpeg = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = "-hide_banner -loglevel error -i pipe:0 -b:a 384k -ac 2 -f s16le -ar 48000 pipe:1 -af loudnorm=I=-16:TP=-1.5:LRA=11:measured_I=-11.8:measured_TP=0.5:measured_LRA=7.8:measured_thresh=-21.9:offset=0:linear=true::print_format=summary",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        YtdLp.Start();
        FFmpeg.Start();

        PipeTask = Task.Run(async () =>
        {
            try
            {
                await YtdLp.StandardOutput.BaseStream.CopyToAsync(FFmpeg.StandardInput.BaseStream);
            }
            catch (Exception ex)
            {
                Console.WriteLine("yt-dlp to ffmpeg pipe error: " + ex.Message);
            }
            finally
            {
                FFmpeg.StandardInput.Close();
            }
        });

        StartLogger();

        return FFmpeg;
    }

    public async Task WaitForStreamAsync()
    {
        if (YtdLp != null && PipeTask != null)
        {
            await YtdLp.WaitForExitAsync();
            await PipeTask;
        }

        YtdLp = null;
        FFmpeg = null;
        PipeTask = null;
    }

    protected void StartLogger()
    {
        Task.Run(TydLpLogger);
        Task.Run(FFmpegLogger);

        return;

        async Task? TydLpLogger()
        {
            while (await YtdLp?.StandardError.ReadLineAsync()! is { } line)
            {
                Console.WriteLine("[yt-dlp ERROR] " + line);
            }
        }

        async Task? FFmpegLogger()
        {
            while (await FFmpeg?.StandardError.ReadLineAsync()! is { } line)
            {
                Console.WriteLine("[ffmpeg ERROR] " + line);
            }
        }
    }

    public async Task<string> GetPreviewUrl(string url)
    {
        var previewPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".gif");

        var ytdLp = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                Arguments = $"-f bestvideo -g \"{url}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };
        ytdLp.Start();
        var output = await ytdLp.StandardOutput.ReadToEndAsync();
        await ytdLp.WaitForExitAsync();

        var ffmpeg = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-ss 0 -t 3 -i \"{output}\" -vf \"scale=720:-1:force_original_aspect_ratio=decrease,fps=10\" -loop 0 -y \"{previewPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        ffmpeg.Start();
        await ffmpeg.WaitForExitAsync();

        return previewPath;
    }

    public async Task<VideoInfo> GetInfo(string url)
    {
        var youtube = new YoutubeClient();

        var video = await youtube.Videos.GetAsync(url);

        return new VideoInfo(video.Title, video.Author.ChannelTitle, video.Author.ChannelUrl, video.Duration?.Ticks ?? 0, video.Url);
    }
}