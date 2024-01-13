﻿using NetCatHook.Scraper.App.Entities;
using NetCatHook.Scraper.App.Repository;
using System.Collections.Concurrent;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using NetCatHook.Scraper.App.NotificationProviders;

namespace NetCatHook.Scraper.App.Telegram;

class TgBotNotificationProvider : INotificationProvider
{
    private readonly ILogger<TgBotNotificationProvider> logger;
    private readonly HttpClient httpClient;
    private readonly IConfiguration configuration;
    private readonly IUnitOfWorkFactory unitOfWorkFactory;
    private TelegramBotClient? botClient;
    private readonly CancellationTokenSource botCts = new();
    private bool disposed = false;
    private ConcurrentDictionary<long, bool> cachedChatIds = new();

    public TgBotNotificationProvider(ILogger<TgBotNotificationProvider> logger,
        HttpClient httpClient,
        IConfiguration configuration, IUnitOfWorkFactory unitOfWorkFactory)
    {
        this.logger = logger;
        this.httpClient = httpClient;
        this.configuration = configuration;
        this.unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task Initialize(CancellationToken cancellationToken)
    {
        logger.LogInformation("Init Tg Bot");
        var secureToken = configuration.GetTgBotSecureToken();
        if (string.IsNullOrWhiteSpace(secureToken))
        {
            throw new ApplicationException("Tg Bot secure token not found");
        }
        botClient = new TelegramBotClient(secureToken, httpClient);

        // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>()
            // receive all update types except ChatMember related updates
        };

        botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: botCts.Token
        );

        var botUser = await botClient.GetMeAsync(cancellationToken);

        cachedChatIds = await LoadBotChatIds();
        logger.LogInformation($"Loaded Tg Bot chats: {cachedChatIds.Count}");

        logger.LogInformation($"Tg Bot @{botUser.Username} started");
    }

    private async Task<ConcurrentDictionary<long, bool>> LoadBotChatIds()
    {
        await using var unitOfWork = unitOfWorkFactory.CreateUnitOfWork();
        var repository = unitOfWork.CreateTgBotChatRepository();
        var chats = await repository.GetAllAsync();
        var chatIdPairs = chats.Select(chat =>
            new KeyValuePair<long, bool>(chat.ChatId, default)).ToArray();

        return new ConcurrentDictionary<long, bool>(chatIdPairs);
    }

    public async Task SendMessage(string message)
    {
        if (disposed || botClient is null || cachedChatIds.IsEmpty)
        {
            return;
        }

        var messageTasks = new List<Task>();
        var copyOfChatIds = cachedChatIds.Keys;
        foreach (var chatId in copyOfChatIds)
        {
            var task = botClient.SendTextMessageAsync(
                chatId: chatId,
                text: message);
            messageTasks.Add(task);
        }

        await Task.WhenAll(messageTasks);
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Only process Message updates: https://core.telegram.org/bots/api#message
        if (update.Message is not { } message)
            return;
        // Only process text messages
        if (message.Text is not { } messageText)
            return;

        var chatId = message.Chat.Id;
        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "You said:\n" + messageText,
            cancellationToken: cancellationToken);

        var isNewChat = cachedChatIds.TryAdd(chatId, default);
        if (isNewChat)
        {
            await SaveChatIfNotExists(chatId);
        }
    }

    private async Task SaveChatIfNotExists(long chatId)
    {
        await using var unitOfWork = unitOfWorkFactory.CreateUnitOfWork();
        var repository = unitOfWork.CreateTgBotChatRepository();

        var savedChat = await repository.GetByChatId(chatId);
        if (savedChat is null)
        {
            var tgBotChat = new TgBotChat() { ChatId = chatId };
            await repository.AddAsync(tgBotChat);
            await unitOfWork.SaveChangesAsync();
        }
    }

    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        logger.LogError(errorMessage);
        return Task.CompletedTask;
    }

    #region IDisposable
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        botCts.Cancel();
        botCts.Dispose();
        botClient = null;
        disposed = true;
        logger.LogInformation("Tg Bot disposed");
    }
    #endregion

}