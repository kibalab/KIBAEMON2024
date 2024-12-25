#nullable enable

using System.Diagnostics;
using Discord.Audio;
using Discord.WebSocket;
using ConnectionState = Discord.ConnectionState;

namespace KIBAEMON2024_Audio;

public class AudioScheduler(ISocketMessageChannel textChannel)
{
    public bool IsPlaying { get; private set; }
    public ulong VoiceChannelId { get; private set; }

    private AudioQueue Queue { get; set; } = new();
    private IAudioClient? AudioClient { get; set; }
    private ISocketMessageChannel TextChannel { get; set; } = textChannel;

    public async Task EnqueueAsync(AudioTrack track, ulong voiceChannelId)
    {
        VoiceChannelId = voiceChannelId;
        Queue.Enqueue(track);
        Console.WriteLine($"Track added: {track.Title}");
        await TextChannel.SendMessageAsync($"`{track.Title}` 을(를) 재생 대기열에 추가했습니다.");

        if (!IsPlaying)
        {
            _ = Task.Run(PlayLoop);
        }
    }

    public void Skip()
    {
        IsPlaying = false;
    }

    public void Stop()
    {
        Queue.Clear();
        IsPlaying = false;
    }

    private async Task PlayLoop()
    {
        IsPlaying = true;

        while (Queue.Any())
        {
            var currentTrack = Queue.Dequeue();
            if (currentTrack == null)
            {
                IsPlaying = false;
                break;
            }

            await TextChannel.SendMessageAsync($"`{currentTrack.Title}` 재생을 시작합니다.");

            var guild = (TextChannel as SocketGuildChannel)?.Guild;
            if (guild == null)
            {
                await TextChannel.SendMessageAsync("길드를 찾을 수 없습니다. 재생을 중단합니다.");
                IsPlaying = false;
                return;
            }

            var voiceChannel = guild.GetVoiceChannel(VoiceChannelId);
            if (voiceChannel == null)
            {
                await TextChannel.SendMessageAsync("음성 채널을 찾을 수 없습니다. 재생을 중단합니다.");
                IsPlaying = false;
                return;
            }

            if (AudioClient == null || AudioClient.ConnectionState != ConnectionState.Connected)
            {
                AudioClient = await voiceChannel.ConnectAsync();
            }

            try
            {
                await using var output = await PlatformManager.StreamSolve(currentTrack.Url);
                await using var discordStream = AudioClient.CreatePCMStream(AudioApplication.Mixed, 96000, packetLoss: 10);

                try
                {
                    await output.CopyToAsync(discordStream);
                }
                finally
                {
                    await discordStream.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                await TextChannel.SendMessageAsync($"재생 중 오류 발생: {ex.Message}");
            }

            if (!Queue.Any())
            {
                IsPlaying = false;
                break;
            }

            if (!IsPlaying)
            {
                IsPlaying = true;
            }
        }

        if (AudioClient != null)
        {
            await AudioClient.StopAsync();
            AudioClient = null;
        }

        IsPlaying = false;
    }

    private static Process CreateFFmpegProcess(string url)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = "-hide_banner -loglevel error -i pipe:0 -b:a 384k -ac 2 -f s16le -ar 48000 pipe:1 -af loudnorm=I=-16:TP=-1.5:LRA=11:measured_I=-11.8:measured_TP=0.5:measured_LRA=7.8:measured_thresh=-21.9:offset=0:linear=true::print_format=summary",
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        return Process.Start(processStartInfo)!;
    }
}