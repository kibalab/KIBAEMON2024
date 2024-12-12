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


        if (context.User is IVoiceState voiceState && voiceState.VoiceChannel != null)
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

        await context.Channel.SendMessageAsync($"재생 시작: {url}");
        await player.PlayAsync(url);
    }

    [Command("music", "현재 재생중인 오디오 정지", "stop")]
    public async Task StopAsync(Bot bot, CommandContext context, CommandParameters parameters)
    {
        var player = bot.GetService<PlayerService>();

        player.StopPlaying();
        await context.Channel.SendMessageAsync("재생을 정지하였습니다.");
    }

    [Command("music", "현재 재생중인 오디오 일시정지/재개", "pause")]
    public async Task PauseAsync(Bot bot, CommandContext context, CommandParameters parameters)
    {
        var player = bot.GetService<PlayerService>();

        if (!player.IsPlaying)
        {
            await context.Channel.SendMessageAsync("현재 재생중인 오디오가 없습니다.");
            return;
        }

        player.Pause();
        if (player.IsPaused)
            await context.Channel.SendMessageAsync("재생을 일시정지하였습니다.");
        else
            await context.Channel.SendMessageAsync("재생을 재개하였습니다.");
    }

    [Command("music", "음성 채널 나가기", "leave")]
    public async Task LeaveAsync(Bot bot, CommandContext context, CommandParameters parameters)
    {
        var player = bot.GetService<PlayerService>();

        await player.LeaveAsync();
        await context.Channel.SendMessageAsync("음성 채널에서 나갔습니다.");
    }
}