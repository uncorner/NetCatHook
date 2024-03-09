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
 
    private static class Commands
    {
        public const string Start = "/start";
        public const string NowWeather = "/now";
        public const string NotificationsOn = "/notificationson";
        public const string NotificationsOff = "/notificationsoff";
    }

    private static class TextCommands
    {
        public const string NowWeather = "Узнать погоду";
        public const string OptionsList = "Показать опции";
    }

    private static class InlineButtonDatas
    {
        public const string NowWeather = "now_weather";
        public const string NotificationsOn = "notifications_on";
        public const string NotificationsOff = "notifications_off";
    }

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

    private static string StartCommandMessage => $"Используйте команду {Commands.NowWeather} чтобы узнать погоду на данный момент.\nАктивируйте уведомления о погоде командой {Commands.NotificationsOn}";

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
                UpdateType.CallbackQuery,
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
            await DoHandleUpdateAsync(botClient, update, cancellationToken);
        }
        catch (Exception ex) {
            logger.LogError($"Error in {nameof(HandleUpdateAsync)}: {ex}");
        }
    }

    private async Task DoHandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        switch (update.Type)
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

                    var chatData = await SaveNewChatIfNoCached(message.Chat.Id);

                    var messageText = message.Text.Trim();
                    if (messageText.StartsWith('/'))
                    {
                        messageText = messageText.ToLowerInvariant();
                    }

                    switch (messageText)
                    {
                        case Commands.Start:
                            await HandleStartCommand(message, cancellationToken);
                            break;
                        case Commands.NowWeather or TextCommands.NowWeather:
                            await HandleNowCommand(message, cancellationToken);
                            break;
                        case TextCommands.OptionsList:
                            await HandleOptionsList(message, cancellationToken, chatData);
                            break;
                        case Commands.NotificationsOn:
                            await HandleNotifications(message, cancellationToken, true);
                            break;
                        case Commands.NotificationsOff:
                            await HandleNotifications(message, cancellationToken, false);
                            break;
                        default:
                            await HandleOptionsList(message, cancellationToken, chatData);
                            break;
                    }

                    break;
                }
            case UpdateType.CallbackQuery:
                {
                    logger.LogInformation($"UpdateType.CallbackQuery, Chat id {update.CallbackQuery?.Message?.Chat.Id}, Data '{update.CallbackQuery?.Data}'");

                    var callbackQuery = update.CallbackQuery;
                    if (callbackQuery?.Message is not { } message)
                    {
                        return;
                    }

                    await SaveNewChatIfNoCached(message.Chat.Id);

                    switch (callbackQuery.Data)
                    {
                        case InlineButtonDatas.NowWeather:
                            {
                                await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
                                await HandleNowCommand(message, cancellationToken);
                                break;
                            }
                        case InlineButtonDatas.NotificationsOn:
                            {
                                await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
                                await HandleNotifications(message, cancellationToken, true);
                                break;
                            }
                        case InlineButtonDatas.NotificationsOff:
                            {
                                await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
                                await HandleNotifications(message, cancellationToken, false);
                                break;
                            }
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
            replyMarkup: GetReplyKeyboard());
    }

    private async Task HandleOptionsList(Message message, CancellationToken cancellationToken, ChatData chatData)
    {
        var notificationsButton = 
            InlineKeyboardButton.WithCallbackData("Активировать уведомления", InlineButtonDatas.NotificationsOn);
        if (chatData.IsNotifying)
        {
            notificationsButton =
                InlineKeyboardButton.WithCallbackData("Деактивировать уведомления", InlineButtonDatas.NotificationsOff);
        }

        var inlineKeyboard = new InlineKeyboardMarkup(
            new List<InlineKeyboardButton[]>()
            {
                new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData("Узнать текущую погоду", InlineButtonDatas.NowWeather),
                },
                new InlineKeyboardButton[]
                {
                    notificationsButton
                },
            });

        await botClient!.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "Выберите действие:",
            cancellationToken: cancellationToken,
            replyMarkup: inlineKeyboard);
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
        await botClient!.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: weatherSummary,
                cancellationToken: cancellationToken,
                replyMarkup: GetReplyKeyboard());
    }

    private async Task HandleStartCommand(Message message, CancellationToken cancellationToken)
    {
        var userName = GetResultUserName(message.From);
        var helloStr = $"Здравствуйте, {userName}\n{StartCommandMessage}";
        await botClient!.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: helloStr,
            cancellationToken: cancellationToken,
            replyMarkup: GetReplyKeyboard());
    }

    private static ReplyKeyboardMarkup GetReplyKeyboard()
    {
        var buttons = new KeyboardButton[][] {
            new KeyboardButton[]
            {
                new KeyboardButton(TextCommands.NowWeather),
                new KeyboardButton(TextCommands.OptionsList)
            },
        };

        return new ReplyKeyboardMarkup(buttons)
        {
            ResizeKeyboard = true
        };
    }

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