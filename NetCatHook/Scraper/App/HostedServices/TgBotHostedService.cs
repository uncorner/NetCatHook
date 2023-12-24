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
    private TelegramBotClient? botClient;
    private CancellationTokenSource botCts = new();
    private IList<long> userChatIds = new List<long>();

    public TgBotHostedService(ILogger<TgBotHostedService> logger,
        WeatherNotifyer weatherNotifyer, HttpClient httpClient)
    {
        this.logger = logger;
        this.weatherNotifyer = weatherNotifyer;
        this.httpClient = httpClient;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Tg Bot");

        Console.WriteLine("Enter Tg secure token:");
        var token = Console.ReadLine();
        botClient = new TelegramBotClient(token?.Trim() ?? "", httpClient);

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

        logger.LogInformation($"Tg Bot @{botUser.Username} started");
    }
    
    private async void HandleWeatherNotifyer(string message)
    {
        if (botClient is null || userChatIds.Count == 0)
        {
            return;
        }

        var messageTasks = new List<Task>();
        foreach(var userChatId in userChatIds)
        {
            var task = botClient.SendTextMessageAsync(
                chatId: userChatId,
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

        userChatIds.Add(chatId);
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
