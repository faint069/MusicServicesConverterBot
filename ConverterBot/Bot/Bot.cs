using System;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;

namespace ConverterBot.Bot
{
  public static class Bot
  {
    public static readonly TelegramBotClient Client;

    static Bot( )
    {
      Log.Information( "Connecting to bot..." );

      Client = new TelegramBotClient( Config.TelegramToken );
      
      Log.Information( "Connected to bot successfully" );
    }
    

    static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
      if (exception is ApiRequestException apiRequestException)
      {
        Log.Error( apiRequestException.ToString());
      }

      return Task.CompletedTask;
    }

    public static void StartReceiving( )
    {
      var receiverOptions = new ReceiverOptions
                            {
                              AllowedUpdates = {}// receive all update types
                            };
      
      Client.StartReceiving( MessageHandler.HandleUpdateAsync, HandleErrorAsync, receiverOptions, CancellationToken.None);
    }
  }
}