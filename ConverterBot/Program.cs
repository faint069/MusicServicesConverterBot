using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ConverterBot.Models;
using ConverterBot.Parsers;
using Serilog;
using Serilog.Events;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;

namespace ConverterBot
{
    class Program
    {
        private static Dictionary<string, IParser> _parsers;
        
        private static void Main( )
        {
            if ( !Directory.Exists( Config.LogPath ) )
            {
                Directory.CreateDirectory( Config.LogPath );
            }

            Log.Logger = new LoggerConfiguration( )
                         .MinimumLevel.Debug( )
                         .MinimumLevel.Override( "Microsoft", LogEventLevel.Warning )
                         .MinimumLevel.Override( "SpotifyAPI.Web", Config.LogLevel )
                         .MinimumLevel.Override( "Telegram.Bot", Config.LogLevel )
                         .MinimumLevel.Override( "Yandex.Music.Api", LogEventLevel.Information )
                         .Enrich.FromLogContext( )
                         .WriteTo.File( $"{Config.LogPath}{DateTime.Now:yyyy-MM-dd-hh_mm_ss}.log",
                            LogEventLevel.Debug )
                         .WriteTo.Console( LogEventLevel.Debug )
                         .CreateLogger( );

            Log.Information( "Загрузка парсеров..." );
            
            _parsers = new Dictionary<string, IParser>
            {
                { "yandex", new YmParser( ) }, 
                { "spotify", new SpotifyParser( ) }
            };
            
            Log.Information( $"Загружено {_parsers.Count} парсера(ов)" );

            Log.Information( "Подключение к боту..." );

            TelegramBotClient bot_client = new TelegramBotClient( Config.TelegramToken );

            bot_client.OnMessage += BotOnMessage;
            bot_client.StartReceiving( );

            Log.Information( "Подключение к боту успешно" );

            while ( true )
            {
                Thread.Sleep( 10000 );
            }
        }

        private static async void BotOnMessage( object? Sender, MessageEventArgs E )
        {
            Log.Information( $"Получено сообщение от {E.Message.Chat.Id} " +
                             $"{E.Message.Chat.FirstName} " +
                             $"{E.Message.Chat.LastName}. " +
                             $"Текст: {E.Message.Text}" );
            
            if ( !( Sender is TelegramBotClient ) )
            {
                throw new ApplicationException( "Ошибка при отправке сообщения." );
            }

            var botclient = ( TelegramBotClient ) Sender;

            if ( E.Message.Text != null )
            {
               string reply = ProcessMessage( E.Message );
               
               await botclient.SendTextMessageAsync( E.Message.Chat.Id, reply );
            }
        }

        private static string ProcessMessage( Message message )
        {

            IParser parser = _parsers.First( _ => message.Text.Contains( _.Key ) ).Value;

            IMusic parsedMusic = parser.ParseUri( message.Text );

            return parsedMusic.ToString( );
        }
    }
}