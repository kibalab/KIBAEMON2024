using System.Diagnostics;
using CliWrap;
using CliWrap.Buffered;
using Discord.Audio;
using Discord.WebSocket;
using KIBAEMON2024_Core.Struct;

namespace KIBAEMON2024_CSharp.Service;

public class PlayerService : IService
{
    private IAudioClient _audioClient;
    private Process _ffmpeg;
    private bool _isPlaying = false;
    private bool _isPaused = false;

    public SocketVoiceChannel CurrentVoiceChannel { get; private set; }

    public async Task JoinAsync(SocketVoiceChannel channel)
    {
        // For the next step with transmitting audio, you would want to pass this Audio Client in to a service.
        _audioClient = await channel.ConnectAsync();
        CurrentVoiceChannel = channel;
    }

    public async Task LeaveAsync()
    {
        StopPlaying();
        if (_audioClient != null)
        {
            await _audioClient.StopAsync();
            _audioClient = null;
            CurrentVoiceChannel = null;
        }
    }

    public void StopPlaying()
    {
        _isPlaying = false;
        _isPaused = false;
        if (_ffmpeg != null && !_ffmpeg.HasExited)
        {
            _ffmpeg.Kill();
            _ffmpeg.Dispose();
            _ffmpeg = null;
        }
    }

    public async Task PlayAsync(string youtubeUrl)
    {
        Console.WriteLine($"Play: {youtubeUrl}");

        _isPlaying = true;
        _isPaused = false;

        // 직접 오디오 스트림 URL 가져오기
        var audioUrl = await GetDirectAudioUrl(youtubeUrl);
        Console.WriteLine($"Audio URL: {audioUrl}");
        if (string.IsNullOrEmpty(audioUrl))
        {
            throw new Exception("오디오 URL을 가져올 수 없습니다. yt-dlp 설정을 확인하세요.");
        }

        SendAsync(_audioClient, audioUrl).Wait();

        return;

        async Task SendAsync(IAudioClient client, string path)
        {
            // Create FFmpeg using the previous example
            using var ffmpeg = CreateStream(path);
            await using var output = ffmpeg.StandardOutput.BaseStream;
            await using var discord = client.CreatePCMStream(AudioApplication.Mixed);

            await output.CopyToAsync(discord);
            await discord.FlushAsync();
        }

        Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });
        }

        async Task<string> GetDirectAudioUrl(string youtubeUrl)
        {
            // yt-dlp로 오디오 스트림 URL만 추출
            // full Command: yt-dlp -f bestaudio -g "https://www.youtube.com/watch
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "yt-dlp",
                    Arguments = $"-f bestaudio -g \"{youtubeUrl}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            return output.Trim();
        }
    }

    public void Pause()
    {
        if (_isPlaying)
        {
            // 단순히 프로세스를 일시정지하는 것은 쉽지 않으나,
            // 여기서는 프로세스를 stop하지 않고 데이터를 읽지 않는 방식으로 "정지"를 모사할 수도 있다.
            // 실제 구현에서는 ffmpeg에 일시정지 명령을 보내거나, 재생 로직을 더 정교하게 구현 필요.
            // 간략히 상태만 토글한다고 가정.
            _isPaused = !_isPaused;
            // 일시정지 구현 예시(실제 동작x): _ffmpeg.Suspend();
        }
    }

    public bool IsPlaying => _isPlaying;
    public bool IsPaused => _isPaused;
}