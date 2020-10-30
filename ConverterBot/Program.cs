#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Text.RegularExpressions;
using System.Threading;
using ConverterBot.Builders;
using ConverterBot.Misc;
using ConverterBot.Models;
using ConverterBot.Parsers;
using Serilog;
using Serilog.Events;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace ConverterBot
{
    static class Program
    {
        private static Dictionary<string, IParser> _parsers = null!;
        private static Dictionary<string, IBuilder> _builders = null!;
        
        private static readonly Regex UriRegex = new Regex(@"((([A-Za-z]{3,9}:(?:\/\/)?)(?:[-;:&=\+\$,\w]+@)?[A-Za-z0-9.-]+|(?:www.|[-;:&=\+\$,\w]+@)[A-Za-z0-9.-]+)((?:\/[\+~%\/.\w-_]*)?\??(?:[-\+=&;%@.\w_]*)#?(?:[.\!\/\\w]*))?)");
        
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

            _parsers = new Dictionary<string, IParser>
            {
                {Services.YandexMusic, new YmParser( )},
                {Services.Spotify,     new SpotifyParser( )}
            };
            
            _builders = new Dictionary<string, IBuilder>
            {
                {Services.YandexMusic,  new YmBuilder(  )},
                {Services.Spotify,      new SpotifyBuilder(  )},
                {Services.YoutubeMusic, new YoutubeMusicBuilder(  )}
            };

            Log.Information( "Connecting to bot..." );

            TelegramBotClient botClient = new TelegramBotClient( Config.TelegramToken );

            botClient.OnMessage += BotOnMessage;
            botClient.OnCallbackQuery += BotOnInlineQuery;
            botClient.StartReceiving( );

            Log.Information( "Connected to bot successfully" );

            while ( true )
            {
                Thread.Sleep( 10000 );
            }
        }



        private static async void BotOnMessage( object? sender, MessageEventArgs e )
        {
            if ( !( sender is TelegramBotClient ) ) throw new ApplicationException( "Ошибка при отправке сообщения." );

            var botClient = ( TelegramBotClient ) sender;

            switch ( e.Message.Type )
            {
                case MessageType.Text:
                {
                    Log.Information( $"Text Message recieved in chat: " +
                                     $"{e.Message.Chat.Id} " +
                                     $"from: {e.Message.From.FirstName} " +
                                     $"{e.Message.From.LastName}. " +
                                     $"Text: {e.Message.Text}" );

                    if ( e.Message.Text.StartsWith( '/' ) )
                    {
                            botClient.SendTextMessageAsync( e.Message.Chat.Id, "Выберите первый сервис", replyMarkup: ProcessCommand( e.Message.Text ));
                        return;
                    }
                    
                    var uri = UriRegex.Match( e.Message.Text ).Value;

                    if ( !string.IsNullOrWhiteSpace( uri ) )
                        try
                        {
                            IParser parser = _parsers.First( _ => e.Message.Text.Contains( _.Key ) ).Value;
                            IMusic  parsedMusic = parser.ParseUri( e.Message.Text );
                            await botClient.SendTextMessageAsync( e.Message.Chat.Id, parsedMusic.ToString( ) );
                            IBuilder builder = _builders.First( _ => !e.Message.Text.Contains( _.Key ) ).Value;
                            string reply = builder.SearchMusic( parsedMusic ) ?? "Не получилось найти музыку";

                            await botClient.SendTextMessageAsync( e.Message.Chat.Id, reply );
                        }
                        catch ( InvalidOperationException )
                        {
                            Log.Error( "Uri received, but parser not found" );
                            await botClient.SendTextMessageAsync( e.Message.Chat.Id,
                                "Либо это не ссылка на музыку, либо я не умею работать с этим сервисом" );
                        }
                        catch ( NullReferenceException )
                        {
                            Log.Error( "Parser failed" );
                            await botClient.SendTextMessageAsync( e.Message.Chat.Id,
                                "Музыка не распознана" );
                        }

                    break;
                }

                case MessageType.VideoNote:
                {
                    Log.Information( "Videomessage received: " +
                                     $"{e.Message.Chat.Id} " +
                                     $"from: {e.Message.From.FirstName} " +
                                     $"{e.Message.From.LastName}.");
                    
                    await botClient.SendStickerAsync( e.Message.Chat.Id,
                                                new InputOnlineFile( Stickers.GetRandomSmickingBotSticker(  ) ) );

                    break;
                }
            }
        }

        private static InlineKeyboardMarkup ProcessCommand( string messageText )
        {
            if ( messageText == "/set_services" )
            {
                List<InlineKeyboardButton> buttons = new List<InlineKeyboardButton>();
                for ( int i = 0; i < 3; i++ )
                {
                    var button = new InlineKeyboardButton(  );
                    button.Text = i.ToString();
                    button.CallbackData = "set_services " + i;
                    buttons.Add( button );
                }
                var keyboard = new InlineKeyboardMarkup( buttons );
                return keyboard;
            }

            return null;
        }
        private static void BotOnInlineQuery( object? sender, CallbackQueryEventArgs callbackQueryEventArgs )
        {
            var a = callbackQueryEventArgs;
        }
    }
}
