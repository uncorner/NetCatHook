using NetCatHook.Scraper.App.Entities;
using NetCatHook.Scraper.App.Repository;
using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NetCatHook.Scraper.App.HostedServices;

class TgBotHostedService : IHostedService
{
    private readonly ILogger<TgBotHostedService> logger;
    private readonly WeatherNotifyer weatherNotifyer;
    private readonly HttpClient httpClient;
    private readonly IConfiguration configuration;
    private readonly IUnitOfWorkFactory unitOfWorkFactory;
    private TelegramBotClient? botClient;
    private CancellationTokenSource botCts = new();
    //private IList<long> userChatIds = new List<long>();

    private ConcurrentDictionary<long, bool> chatIds = new();

    public TgBotHostedService(ILogger<TgBotHostedService> logger,
        WeatherNotifyer weatherNotifyer, HttpClient httpClient,
        IConfiguration configuration, IUnitOfWorkFactory unitOfWorkFactory)
    {
        this.logger = logger;
        this.weatherNotifyer = weatherNotifyer;
        this.httpClient = httpClient;
        this.configuration = configuration;
        this.unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Tg Bot");
        botClient = new TelegramBotClient(configuration.GetTgBotSecureToken(), httpClient);

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
        weatherNotifyer.Event += HandleWeatherNotifyer;

        //>>>>
        await using(var unitOfWork = unitOfWorkFactory.CreateUnitOfWork())
        {
            var repository = unitOfWork.CreateTgBotChatRepository();
            var chats = await repository.GetAllAsync();
            var chatIdPairs = chats.Select(chat =>
                new KeyValuePair<long, bool>(chat.ChatId, default));

            chatIds = new ConcurrentDictionary<long, bool>(chatIdPairs);
        }

        logger.LogInformation($"Tg Bot @{botUser.Username} started");
    }
    
    private async void HandleWeatherNotifyer(string message)
    {
        if (botClient is null || chatIds.Count == 0)
        {
            return;
        }

        var messageTasks = new List<Task>();
        var copyOfChatIds = chatIds.Keys;
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

        //chatIds.Add(chatId);
        // >>>>>>>>>>>
        var isNewChat = chatIds.TryAdd(chatId, default);
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

    public Task StopAsync(CancellationToken cancellationToken)
    {
        weatherNotifyer.Event -= HandleWeatherNotifyer;
        botCts.Cancel();
        botCts.Dispose();

        logger.LogInformation("Tg Bot stopped");
        return Task.CompletedTask;
    }

}
