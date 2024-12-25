#nullable enable

using Discord;
using Discord.WebSocket;

namespace KIBAEMON2024_CSharp.Extension;

public static class ChannelExtensions
{
    public static bool TryGetGuild(this ISocketMessageChannel channel, out SocketGuild? guild)
    {
        guild = (channel as SocketGuildChannel)?.Guild;
        return guild is not null;
    }

    public static bool TryGetVoiceChannel(this SocketGuild guild, ulong voiceChannelId, out SocketVoiceChannel? voiceChannel)
    {
        voiceChannel = guild.GetVoiceChannel(voiceChannelId);
        return voiceChannel is not null;
    }
}