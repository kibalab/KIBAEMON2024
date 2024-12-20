using Discord;
using Discord.WebSocket;
using KIBAEMON2024_Core;
using KIBAEMON2024_CSharp.Service;
using KIBAEMON2024_CSharp.System;

namespace KIBAEMON2024_CSharp.Command;

public class MusicCommands : ICommandGroup
{
    [Command("music", "사용자가 속한 음성 채널에 봇 접속", "join")]
    public async Task JoinAsync(Bot bot, CommandContext context, CommandParameters parameters)
    {
        var player = bot.GetService<PlayerService>();

        var id = parameters.Get<string>("text");

        if (!string.IsNullOrEmpty(id) && context.Guild != null)
        {
            var channel = await context.Guild.GetVoiceChannelAsync(ulong.Parse(id));
            if (channel != null)
            {
                await player.JoinAsync((SocketVoiceChannel)channel);
                await context.Channel.SendMessageAsync("음성 채널에 접속하였습니다.");
            }
            else
            {
                await context.Channel.SendMessageAsync("음성 채널을 찾을 수 없습니다.");
            }

            return;
        }


        if (context.User is IVoiceState { VoiceChannel: not null } voiceState)
        {
            await player.JoinAsync((SocketVoiceChannel)voiceState.VoiceChannel);
            await context.Channel.SendMessageAsync("음성 채널에 접속하였습니다.");
        }
        else
        {
            await context.Channel.SendMessageAsync("먼저 음성 채널에 접속해주세요.");
        }
    }

    [Command("music", "YouTube URL을 재생", "play")]
    public async Task PlayAsync(Bot bot, CommandContext context, CommandParameters parameters)
    {
        var player = bot.GetService<PlayerService>();

        var url = parameters.Get<string>("text");
        if (string.IsNullOrEmpty(url))
        {
            await context.Channel.SendMessageAsync("재생할 유튜브 URL을 입력해주세요.");
            return;
        }

        // 길드 ID 획득
        var guildId = (context.Guild as SocketGuild)?.Id;
        if (guildId == null)
        {
            await context.Channel.SendMessageAsync("길드에서만 사용할 수 있는 명령입니다.");
            return;
        }

        var playerContext = player.GetContext(guildId.Value);

        if (playerContext == null || playerContext.AudioClient == null)
        {
            if (context.User is IVoiceState { VoiceChannel: not null } voiceState)
            {
                await player.JoinAsync((SocketVoiceChannel)voiceState.VoiceChannel);
            }
            else
            {
                await context.Channel.SendMessageAsync("먼저 음성 채널에 접속해주세요.");
                return;
            }
        }

        var provider = player.StreamProvider.GetProvider(url);
        var previewUrl = await provider.GetPreviewUrl(url);
        var videoInfo = await provider.GetInfo(url);
        var duration = new TimeSpan(videoInfo.Duration);
        var playingText = $"재생할 영상: {videoInfo.Title} - {videoInfo.Author} ({duration:mm\\:ss})";

        await using (var fileStream = File.OpenRead(previewUrl))
        {
            await context.Channel.SendFileAsync(fileStream, "preview.gif", playingText);
        }

        await player.PlayAsync(guildId.Value, url);
    }

    [Command("music", "재생 중지", "stop")]
    public async Task StopAsync(Bot bot, CommandContext context, CommandParameters parameters)
    {
        var player = bot.GetService<PlayerService>();
        var guildId = (context.Guild as SocketGuild)?.Id;

        if (guildId == null)
        {
            await context.Channel.SendMessageAsync("길드에서만 사용할 수 있습니다.");
            return;
        }

        player.StopPlaying(guildId.Value);
        await context.Channel.SendMessageAsync("재생을 중지했습니다.");
    }

    [Command("music", "현재 재생중인 오디오 일시정지/재개", "pause")]
    public async Task PauseAsync(Bot bot, CommandContext context, CommandParameters parameters)
    {
        var player = bot.GetService<PlayerService>();
        var guildId = (context.Guild as SocketGuild)?.Id;
        var playerContext = player.GetContext(guildId.GetValueOrDefault());

        if (guildId == null)
        {
            await context.Channel.SendMessageAsync("길드에서만 사용할 수 있습니다.");
            return;
        }

        if (playerContext == null || playerContext.AudioClient == null)
        {
            await context.Channel.SendMessageAsync("먼저 음성 채널에 접속해주세요.");
            return;
        }

        if (playerContext.IsPaused)
        {
            await context.Channel.SendMessageAsync("현재 재생중인 오디오가 없습니다.");
            return;
        }

        player.Pause(guildId.Value);
        if (playerContext.IsPaused)
            await context.Channel.SendMessageAsync("재생을 일시정지하였습니다.");
        else
            await context.Channel.SendMessageAsync("재생을 재개하였습니다.");
    }

    [Command("music", "음성 채널 나가기", "leave")]
    public async Task LeaveAsync(Bot bot, CommandContext context, CommandParameters parameters)
    {
        var player = bot.GetService<PlayerService>();
        var guildId = (context.Guild as SocketGuild)?.Id;

        if (guildId == null)
        {
            await context.Channel.SendMessageAsync("길드에서만 사용할 수 있습니다.");
            return;
        }

        await player.LeaveAsync(guildId.Value);
        await context.Channel.SendMessageAsync("음성 채널에서 나갔습니다.");
    }
}