using System.Diagnostics;

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
                Arguments = "-hide_banner -loglevel error -i pipe:0 -b:a 128k -ac 2 -f s16le -ar 48000 pipe:1 -af loudnorm=I=-16:TP=-1.5:LRA=11:measured_I=-11.8:measured_TP=0.5:measured_LRA=7.8:measured_thresh=-21.9:offset=0:linear=true::print_format=summary",
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
}