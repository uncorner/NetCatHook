using NetCatHook.Scraper.App.Entities;
using NetCatHook.Scraper.App.Repository;
using System.Collections.Concurrent;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using NetCatHook.Scraper.App;
using NetCatHook.Scraper.App.Messenger;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;

namespace NetCatHook.Scraper.Infrastructure.Messenger;

class TgBotMessenger : IMessenger
{
    private readonly ILogger<TgBotMessenger> logger;
    private readonly IConfiguration configuration;
    private readonly IUnitOfWorkFactory unitOfWorkFactory;
    private readonly IHttpClientFactory httpClientFactory;
    private ITelegramBotClient? botClient;
    private readonly CancellationTokenSource botCts = new();
    private bool disposed = false;
    private ConcurrentDictionary<long, ChatData> cachedChats = new();
    private IWeatherInformer? weatherInformer = null!;
    private const string NowCommandMessage = "Используйте команду /now чтобы узнать погоду на данный момент";
    private const string NowCommandMessageText = "Узнать погоду";

    public TgBotMessenger(ILogger<TgBotMessenger> logger,
        IConfiguration configuration,
        IUnitOfWorkFactory unitOfWorkFactory,
        IHttpClientFactory httpClientFactory)
    {
        this.logger = logger;
        this.configuration = configuration;
        this.unitOfWorkFactory = unitOfWorkFactory;
        this.httpClientFactory = httpClientFactory;
    }

    public async Task Initialize(CancellationToken cancellationToken)
    {
        logger.LogInformation("Init Tg Bot");
        var secureToken = configuration.GetTgBotSecureToken();
        if (string.IsNullOrWhiteSpace(secureToken))
        {
            throw new ApplicationException("Tg Bot secure token not found");
        }

        var httpClient = httpClientFactory.CreateClient();
        botClient = new TelegramBotClient(secureToken, httpClient);

        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = new[] {
                UpdateType.Message,
                //UpdateType.CallbackQuery // Inline кнопки
                UpdateType.MyChatMember,
            },
            // if true do not process updates when offline
            ThrowPendingUpdates = true
        };

        botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: botCts.Token
        );

        var botUser = await botClient.GetMeAsync(cancellationToken);

        cachedChats = await LoadBotChats();
        logger.LogInformation($"Loaded Tg Bot chats: {cachedChats.Count}");

        logger.LogInformation($"Tg Bot @{botUser.Username} started");
    }

    private async Task<ConcurrentDictionary<long, ChatData>> LoadBotChats()
    {
        var chatIdMap = Array.Empty<KeyValuePair<long, ChatData>>();
        await using (var unitOfWork = unitOfWorkFactory.CreateUnitOfWork())
        {
            var repository = unitOfWork.CreateTgBotChatRepository();
            var chats = await repository.GetAllEnabled();
            chatIdMap = chats.Select(chat =>
                new KeyValuePair<long, ChatData>(chat.ChatId, ConvertToChatData(chat)))
                .ToArray();
        }

        return new ConcurrentDictionary<long, ChatData>(chatIdMap);
    }

    private static ChatData ConvertToChatData(TgBotChat chat) => new ChatData
    {
        ChatId = chat.ChatId,
        //IsEnabled = chat.IsEnabled,
        IsNotifying = chat.IsNotifying
    };

    public async Task Send(string message)
    {
        if (disposed)
        {
            return;
        }

        await SendMessageToAllChats(message);
    }

    private async Task SendMessageToAllChats(string message)
    {
        if (botClient is null || cachedChats.IsEmpty)
        {
            return;
        }

        var messageTasks = new List<Task>();
        var notifyingChats = cachedChats.Values.Where(i => i.IsNotifying).ToArray();
        if (!notifyingChats.Any())
        {
            return;
        }

        foreach (var chat in notifyingChats)
        {
            var task = botClient.SendTextMessageAsync(
                chatId: chat.ChatId,
                text: message);
            messageTasks.Add(task);
        }

        await Task.WhenAll(messageTasks);
    }

    private static string GetResultUserName(User? user)
    {
        const string defaultUserName = "Пользователь";
        if (user is null)
        {
            return defaultUserName;
        }

        StringBuilder name = new();
        if (user.FirstName is not null)
        {
            name.Append(user.FirstName);
        }
        if (user.LastName is not null)
        {
            name.Append(" " + user.LastName);
        }

        if (name.Length == 0)
        {
            if (user.Username is not null)
            {
                name.Append(user.Username);
            }
            else
            {
                name.Append(defaultUserName);
            }
        }

        return name.ToString();
    }
    
    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            switch(update.Type)
            {
                case UpdateType.Message:
                    {
                        logger.LogInformation($"UpdateType.Message, Chat id {update.Message?.Chat.Id}, Text '{update.Message?.Text}'");

                        if (update.Message is not { } message)
                        {
                            return;
                        }
                        if (string.IsNullOrWhiteSpace(message.Text))
                        {
                            return;
                        }

                        _ = await SaveNewChatIfNoCached(message.Chat.Id);

                        var messageText = message.Text.Trim();
                        if (messageText.StartsWith('/'))
                        {
                            messageText = messageText.ToLowerInvariant();
                        }

                        switch (messageText)
                        {
                            case "/start":
                                await HandleStartCommand(message, cancellationToken);
                                break;
                            case "/now" or NowCommandMessageText:
                                await HandleNowCommand(message, cancellationToken);
                                break;
                            case "/notificationson":
                                await HandleNotifications(message, cancellationToken, true);
                                break;
                            case "/notificationsoff":
                                await HandleNotifications(message, cancellationToken, false);
                                break;
                            default:
                                await HandleOtherMessage(message, cancellationToken);
                                break;
                        }

                        break;
                    }
                case UpdateType.MyChatMember:
                    {
                        if (update.MyChatMember is not { } cmUpdated)
                        {
                            return;
                        }

                        await HandleMyChatMemberUpdated(cmUpdated);
                        break;
                    }
                default:
                    logger.LogWarning($"Unhandled Update type: {update.Type}");
                    break;
            }
        }
        catch (Exception ex) {
            logger.LogError($"Error in {nameof(HandleUpdateAsync)}: {ex}");
        }
    }

    private async Task HandleNotifications(Message message, CancellationToken cancellationToken, bool isNotifying)
    {
        var chatData = await UpdateChat(message.Chat.Id, (chat) =>
        {
            chat.IsNotifying = isNotifying;
        });
        AddOrUpdateCachedChat(chatData);

        await botClient!.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: isNotifying ? "Включены уведомления о погоде" : "Отключены уведомления о погоде",
            cancellationToken: cancellationToken,
            replyMarkup: GetKeyboardNowCommand());
    }

    private async Task HandleOtherMessage(Message message, CancellationToken cancellationToken)
    {
        _ = await botClient!.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: NowCommandMessage,
            cancellationToken: cancellationToken,
            replyMarkup: GetKeyboardNowCommand());
    }

    private async Task HandleMyChatMemberUpdated(ChatMemberUpdated cmUpdated)
    {
        if (cmUpdated is { OldChatMember.Status: ChatMemberStatus.Member,
            NewChatMember.Status: ChatMemberStatus.Kicked or ChatMemberStatus.Left } )
        {
            // chat disabled
            logger.LogInformation($"{nameof(HandleMyChatMemberUpdated)}, save chat id {cmUpdated.Chat.Id} as Disabled");
            await SaveOrUpdateChat(cmUpdated.Chat.Id, (chat) =>
            {
                chat.IsEnabled = false;
            });
            cachedChats.TryRemove(cmUpdated.Chat.Id, out _);
        }
        else if(cmUpdated is { OldChatMember.Status: ChatMemberStatus.Kicked or ChatMemberStatus.Left,
            NewChatMember.Status: ChatMemberStatus.Member })
        {
            // chat enebled
            logger.LogInformation($"{nameof(HandleMyChatMemberUpdated)}, chat id {cmUpdated.Chat.Id} Enabled");
            await SaveNewChatIfNoCached(cmUpdated.Chat.Id);
        }
    }

    private async Task HandleNowCommand(Message message, CancellationToken cancellationToken)
    {
        var weatherSummary = weatherInformer!.GetWeatherSummary();
        _ = await botClient!.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: weatherSummary,
                cancellationToken: cancellationToken,
                replyMarkup: GetKeyboardNowCommand());
    }

    private async Task HandleStartCommand(Message message, CancellationToken cancellationToken)
    {
        var userName = GetResultUserName(message.From);
        var helloStr = $"Здравствуйте, {userName}\n{NowCommandMessage}";
        _ = await botClient!.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: helloStr,
            cancellationToken: cancellationToken,
            replyMarkup: GetKeyboardNowCommand());
    }

    private static ReplyKeyboardMarkup GetKeyboardNowCommand()
    {
        return new ReplyKeyboardMarkup(
            new KeyboardButton[]
            {
                new KeyboardButton(NowCommandMessageText)
            })
        {
            ResizeKeyboard = true
        };
    }

    //private async Task UpdateChat(long chatId, Action<TgBotChat> updateAction)
    //{
    //    await using var unitOfWork = unitOfWorkFactory.CreateUnitOfWork();
    //    var repository = unitOfWork.CreateTgBotChatRepository();
    //    var chat = await repository.GetByChatId(chatId);
    //        //?? throw new Exception($"Tg chat with id {chatId} not found");

    //    if (chat is null)
    //    {
    //        return;
    //    }
    //    updateAction(chat);
    //    await unitOfWork.SaveChangesAsync();
    //}

    //private async Task<TgBotChat> SaveNewChatOrUpdateEnabled(long chatId)
    //{
    //    TgBotChat? chat;
    //    await using (var unitOfWork = unitOfWorkFactory.CreateUnitOfWork())
    //    {
    //        var repository = unitOfWork.CreateTgBotChatRepository();

    //        chat = await repository.GetByChatId(chatId);
    //        if (chat is null)
    //        {
    //            chat = new TgBotChat()
    //            {
    //                ChatId = chatId
    //            };
    //            await repository.Add(chat);
    //        }

    //        chat.IsEnabled = true;
    //        await unitOfWork.SaveChangesAsync();
    //    }

    //    return chat;
    //}

    private async Task<ChatData> UpdateChat(long chatId, Action<TgBotChat> updateAction)
    {
        await using var unitOfWork = unitOfWorkFactory.CreateUnitOfWork();
        var repository = unitOfWork.CreateTgBotChatRepository();
        var chat = await repository.GetByChatId(chatId) ?? throw new Exception($"Tg chat with id {chatId} not found");

        updateAction(chat);
        await unitOfWork.SaveChangesAsync();

        return ConvertToChatData(chat);
    }

    private async Task<ChatData> SaveOrUpdateChat(long chatId, Action<TgBotChat> updateAction)
    {
        await using var unitOfWork = unitOfWorkFactory.CreateUnitOfWork();
        var repository = unitOfWork.CreateTgBotChatRepository();
        var chat = await repository.GetByChatId(chatId);

        if (chat is null)
        {
            chat = new TgBotChat()
            {
                ChatId = chatId,
            };
            await repository.Add(chat);
        }
 
        updateAction(chat);
        await unitOfWork.SaveChangesAsync();
        return ConvertToChatData(chat);
    }

    private async Task<ChatData> SaveNewChatIfNoCached(long chatId)
    {
        var isInCache = cachedChats.TryGetValue(chatId, out var chatData);
        if (!isInCache)
        {
            logger.LogInformation($"Save new chat id {chatId}");
            chatData = await SaveOrUpdateChat(chatId, (chat) =>
            {
                chat.SetDefault();
                chat.IsEnabled = true;
            });

            AddOrUpdateCachedChat(chatData);
        }

        return chatData!;
    }

    private void AddOrUpdateCachedChat(ChatData chatData) =>
        cachedChats.AddOrUpdate(chatData.ChatId, chatData,
            (chatId, theChatData) => chatData);

    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException =>
                $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        logger.LogError(errorMessage);
        return Task.CompletedTask;
    }

    public void SetWeatherInformer(IWeatherInformer weatherInformer)
    {
        this.weatherInformer = weatherInformer;
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