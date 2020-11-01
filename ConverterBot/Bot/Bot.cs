using Serilog;
using Telegram.Bot;

namespace ConverterBot.Bot
{
    public static class Bot
    {
        public static TelegramBotClient Client;

        static Bot( )
        {
            Client = new TelegramBotClient( Config.TelegramToken );
            
            Log.Information( "Connecting to bot..." );

            Client.OnMessage += MessageHandler.BotOnMessage;
            Client.OnCallbackQuery += MessageHandler.BotOnInlineQuery;

            Log.Information( "Connected to bot successfully" );
        }
    }
}