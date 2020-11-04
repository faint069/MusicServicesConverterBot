using Serilog;
using Telegram.Bot;

namespace ConverterBot.Bot
{
  public static class Bot
  {
    public static readonly TelegramBotClient Client;

    static Bot( )
    {
      Log.Information( "Connecting to bot..." );

      Client = new TelegramBotClient( Config.TelegramToken );
            
      Client.OnMessage += MessageHandler.BotOnMessage;
      Client.OnCallbackQuery += MessageHandler.BotOnInlineQuery;

      Log.Information( "Connected to bot successfully" );
    }
  }
}