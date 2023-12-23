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
    private TelegramBotClient? botClient;
    private CancellationTokenSource cts = new();
    private IList<long> userChatIds = new List<long>();

    public TgBotHostedService(ILogger<TgBotHostedService> logger, WeatherNotifyer weatherNotifyer)
    {
        this.logger = logger;
        this.weatherNotifyer = weatherNotifyer;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting TgBotHostedService");

        Console.WriteLine("Enter TG secure token:");
        var token = Console.ReadLine();
        // todo: pass HttpClient
        botClient = new TelegramBotClient(token?.Trim() ?? "");

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
            cancellationToken: cts.Token
        );

        var me = await botClient.GetMeAsync();

        //Console.WriteLine($"Start listening for @{me.Username}");
        //Console.ReadLine();

        // Send cancellation request to stop bot
        //cts.Cancel();
        
        logger.LogInformation($"TgBotHostedService started: @{me.Username}");

        weatherNotifyer.Event += WeatherNotifyer_Event;
    }
    
    private async void WeatherNotifyer_Event(string message)
    {
        if (botClient is null)
        {
            return;
        }

        foreach(var userChatId in userChatIds)
        {
            await botClient.SendTextMessageAsync(
            chatId: userChatId,
            text: message);
        }
    }

    async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Only process Message updates: https://core.telegram.org/bots/api#message
        if (update.Message is not { } message)
            return;
        // Only process text messages
        if (message.Text is not { } messageText)
            return;

        var chatId = message.Chat.Id;
        Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

        // Echo received message text
        Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "You said:\n" + messageText,
            cancellationToken: cancellationToken);

        userChatIds.Add(chatId);
    }

    Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage =
        exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        //todo
        weatherNotifyer.Event -= WeatherNotifyer_Event;
        cts.Cancel();
        cts.Dispose();
        
        logger.LogInformation("TgBotHostedService stopped");
        return Task.CompletedTask;
    }

}
