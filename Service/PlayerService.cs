using Discord;
using Discord.Audio;
using Discord.WebSocket;
using KIBAEMON2024_Core.Struct;

namespace KIBAEMON2024_CSharp.Service
{
    public class PlayerContext
    {
        public IAudioClient? AudioClient { get; set; }
        public IVoiceChannel? VoiceChannel { get; set; }
        public bool IsPlaying { get; set; }
        public bool IsPaused { get; set; }
        public CancellationTokenSource? CancellationTokenSource { get; set; }
    }

    public class PlayerService : IService
    {
        private Dictionary<ulong, PlayerContext> PlayerContexts { get; set; } = new();

        public StreamProviderManager StreamProvider { get; } = new();

        public async Task JoinAsync(SocketVoiceChannel channel)
        {
            var guildId = channel.Guild.Id;
            if (!PlayerContexts.TryGetValue(guildId, out var context))
            {
                context = new PlayerContext();
                PlayerContexts[guildId] = context;
            }

            if (context.AudioClient != null)
            {
                await LeaveAsync(guildId);
            }

            var client = await channel.ConnectAsync(external: false);
            context.AudioClient = client;
            context.VoiceChannel = channel;
        }

        public async Task LeaveAsync(ulong guildId)
        {
            if (PlayerContexts.TryGetValue(guildId, out var context))
            {
                StopPlaying(guildId);
                if (context.AudioClient != null)
                {
                    await context.AudioClient.StopAsync();
                    context.AudioClient = null;
                    context.VoiceChannel = null;
                }
            }
        }

        public void StopPlaying(ulong guildId)
        {
            if (PlayerContexts.TryGetValue(guildId, out var context))
            {
                context.IsPlaying = false;
                context.IsPaused = false;
                context.CancellationTokenSource?.Cancel(); // 재생중인 Task 취소 요청
                context.CancellationTokenSource?.Dispose();
                context.CancellationTokenSource = null;
            }
        }

        public async Task PlayAsync(ulong guildId, string youtubeUrl)
        {
            if (!PlayerContexts.TryGetValue(guildId, out var context))
            {
                throw new Exception("해당 길드에 대한 컨텍스트가 없습니다. 먼저 Join을 수행하세요.");
            }

            if (context.AudioClient == null)
            {
                throw new Exception("음성 채널에 봇이 접속되어 있지 않습니다.");
            }

            context.IsPlaying = true;
            context.IsPaused = false;

            // Stop 명령 시 사용할 CancellationTokenSource
            context.CancellationTokenSource = new CancellationTokenSource();

            // 재생 Task 시작
            await StreamAudioAsync(context, youtubeUrl, context.CancellationTokenSource.Token);
        }

        private async Task StreamAudioAsync(PlayerContext context, string url, CancellationToken cancellationToken)
        {
            var client = context.AudioClient;

            if (client is null)
            {
                return;
            }

            var streamProvider = StreamProvider.GetProvider(url);

            var audioProcess = streamProvider.StartStream(url);
            var discordStream = client.CreatePCMStream(AudioApplication.Mixed, 96000, packetLoss: 10);

            context.IsPlaying = true;
            {
                var discordStreamTask = Task.Run(async () =>
                {
                    try
                    {
                        await audioProcess.StandardOutput.BaseStream.CopyToAsync(discordStream, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("ffmpeg to discord pipe error: " + ex.Message);
                    }
                    finally
                    {
                        await discordStream.FlushAsync(cancellationToken);
                        await discordStream.DisposeAsync();
                    }
                }, cancellationToken);

                await streamProvider.WaitForStreamAsync();
                await discordStreamTask;
                await audioProcess.WaitForExitAsync(cancellationToken);
            }
            context.IsPlaying = false;
        }

        public void Pause(ulong guildId)
        {
            if (PlayerContexts.TryGetValue(guildId, out var context))
            {
                if (context.IsPlaying)
                {
                    // 실제 오디오 일시정지는 여기서 추가 구현 필요

                    context.IsPaused = !context.IsPaused;
                }
            }
        }

        public PlayerContext? GetContext(ulong guildId)
        {
            PlayerContexts.TryGetValue(guildId, out var context);
            return context;
        }
    }
}