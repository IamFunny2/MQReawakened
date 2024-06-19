﻿using A2m.Server;
using Discord;
using Discord.WebSocket;
using Server.Base.Core.Abstractions;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Core.Services;
public class DiscordHandler(DiscordRwConfig rwConfig, PlayerContainer playerContainer) : IService
{
    private DiscordSocketClient _socketClient;

    public void Initialize()
    {
        if (string.IsNullOrEmpty(rwConfig.DiscordBotToken))
            return;

        _socketClient = new DiscordSocketClient();
        _socketClient.MessageReceived += ClientOnMessageReceived;

        _socketClient.LoginAsync(TokenType.Bot, rwConfig.DiscordBotToken);
        _socketClient.StartAsync();
    }

    public void SendMessage(string author, string message)
    {
        if (_socketClient == null)
            return;

        var socketChannel = (ISocketMessageChannel)_socketClient.GetChannel(rwConfig.ChannelId);

        socketChannel.SendMessageAsync(author + ": " + message);
    }

    private async Task ClientOnMessageReceived(SocketMessage socketMessage) =>
        await Task.Run(() =>
        {
            if (!socketMessage.Author.IsBot)
            {
                var author = socketMessage.Author.Username;
                var channelId = socketMessage.Channel.Id;
                var socketChannel = (ISocketMessageChannel)_socketClient.GetChannel(channelId);

                var messageId = socketMessage.Id;
                var message = socketChannel.GetMessageAsync(messageId);

                if (rwConfig.ChannelId != socketChannel.Id)
                    return;

                Log(message.Result.Content, author);
            }
        });

    private void Log(string message, string author)
    {
        lock (PlayerContainer.Lock)
        {
            foreach (
                var client in
                from client in playerContainer.GetAllPlayers()
                select client
            )
                client.Chat(CannedChatChannel.Tell, "Discord > " + author, message);
        }
    }
}
