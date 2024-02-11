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
    private ConcurrentDictionary<long, bool> cachedChatIds = new();
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
            // do not process updates when offline
            ThrowPendingUpdates = true
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
        var chats = await repository.GetAllEnabled();
        var chatIdPairs = chats.Select(chat =>
            new KeyValuePair<long, bool>(chat.ChatId, default)).ToArray();

        return new ConcurrentDictionary<long, bool>(chatIdPairs);
    }

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
        if (botClient is null || cachedChatIds.IsEmpty)
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
                    logger.LogInformation($"UpdateType.Message, Chat id {update.Message?.Chat.Id}, Text '{update.Message?.Text}'");

                    if (update.Message is not { } message)
                    {
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(message.Text))
                    {
                        return;
                    }

                    await SaveChatIfNoCached(message.Chat.Id);

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
                        default:
                            await HandleOtherMessage(message, cancellationToken);
                            break;
                    }

                    break;
                case UpdateType.MyChatMember:
                    if (update.MyChatMember is not { } memberUpdated)
                    {
                        return;
                    }
                    await HandleMyChatMemberUpdated(memberUpdated);
                    break;
                default:
                    logger.LogWarning($"Unhandled Update type: {update.Type}");
                    break;
            }
        }
        catch (Exception ex) {
            logger.LogError($"Error in {nameof(HandleUpdateAsync)}: {ex}");
        }
    }

    private async Task HandleOtherMessage(Message message, CancellationToken cancellationToken)
    {
        _ = await botClient!.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: NowCommandMessage,
            cancellationToken: cancellationToken,
            replyMarkup: GetKeyboardNowCommand());
    }

    private async Task HandleMyChatMemberUpdated(ChatMemberUpdated updated)
    {
        if (updated is { OldChatMember.Status: ChatMemberStatus.Member,
            NewChatMember.Status: ChatMemberStatus.Kicked or ChatMemberStatus.Left } )
        {
            logger.LogInformation($"{nameof(HandleMyChatMemberUpdated)}, save chat id {updated.Chat.Id} as Disabled");
            cachedChatIds.TryRemove(updated.Chat.Id, out _);
            await SaveChat(updated.Chat.Id, false);
        }
        else if(updated is { OldChatMember.Status: ChatMemberStatus.Kicked or ChatMemberStatus.Left,
            NewChatMember.Status: ChatMemberStatus.Member })
        {
            logger.LogInformation($"{nameof(HandleMyChatMemberUpdated)}, chat id {updated.Chat.Id} Enabled");
            await SaveChatIfNoCached(updated.Chat.Id);
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

    private async Task SaveChat(long chatId, bool isEnebled)
    {
        await using var unitOfWork = unitOfWorkFactory.CreateUnitOfWork();
        var repository = unitOfWork.CreateTgBotChatRepository();

        var chat = await repository.GetByChatId(chatId);
        if (chat is null)
        {
            var newChat = new TgBotChat() {
                ChatId = chatId, IsEnabled = true
            };
            await repository.Add(newChat);
        }
        else
        {
            chat.IsEnabled = isEnebled;
        }

        await unitOfWork.SaveChangesAsync();
    }

    private async Task SaveChatIfNoCached(long chatId)
    {
        var isNew = cachedChatIds.TryAdd(chatId, default);
        if (isNew)
        {
            logger.LogInformation($"Save new chat id {chatId}");
            await SaveChat(chatId, true);
        }
    }

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