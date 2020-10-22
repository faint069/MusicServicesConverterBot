using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using ConverterBot.Builders;
using ConverterBot.Models;
using ConverterBot.Parsers;
using Serilog;
using Serilog.Events;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace ConverterBot
{
    class Program
    {
        private static Dictionary<string, IParser> _parsers;
        private static Dictionary<string, IBuilder> _builders;
        
        private static Regex uriRegex = new Regex(@"((([A-Za-z]{3,9}:(?:\/\/)?)(?:[-;:&=\+\$,\w]+@)?[A-Za-z0-9.-]+|(?:www.|[-;:&=\+\$,\w]+@)[A-Za-z0-9.-]+)((?:\/[\+~%\/.\w-_]*)?\??(?:[-\+=&;%@.\w_]*)#?(?:[.\!\/\\w]*))?)");
        
        private static readonly List<string> StickerIds = new List<string>
        {
            "CAACAgQAAxkBAAIFDV-Jls0xIhjVz1MD2hTNNfhmQpD9AAIrAAOYNXECYh2sqTkGNV0bBA",
            "CAACAgQAAxkBAAIFDl-JltHZEOnaDE9rcFE8HyOmDkVAAAKpBgACb2HkAb0rq2EWtocjGwQ",
            "CAACAgIAAxkBAAIFEV-JlvnufJxc7h5GbU6DNb6faJiPAAJXZgACns4LAAEwO23SmGu_oBsE",
            "CAACAgQAAxkBAAIFEl-Jlxqh4qAZEiMK1uGKNGvlULrOAAITAAOYNXECneTXFMXI1zIbBA",
            "CAACAgQAAxkBAAIFE1-JlyBE3PGLz99jlIHZqPWhZahIAAIhAAOYNXECQ1DAYZ5umEQbBA",
            "CAACAgIAAxkBAAIFFl-Jl4wCWT6gvwwAAe_uBeJMbnOscgACFwMAAs-71A59adsQhEjPrRsE",
            "CAACAgIAAxkBAAIFF1-Jl5fJ4fQtLx9ss6PRp30Z7rNjAAIbAwACz7vUDsIc3bMyqex1GwQ"
        };

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
                {"yandex", new YmParser( )},
                {"spotify", new SpotifyParser( )}
            };
            _builders = new Dictionary<string, IBuilder>
            {
                {"yandex" , new YmBuilder(  )},
                {"spotify", new SpotifyBuilder(  )}
            };

            Log.Information( "Connecting to bot..." );

            TelegramBotClient botClient = new TelegramBotClient( Config.TelegramToken );

            botClient.OnMessage += BotOnMessage;
            botClient.StartReceiving( );

            Log.Information( "Connected to bot successfully" );

            while ( true )
            {
                Thread.Sleep( 10000 );
            }
        }


        private static async void BotOnMessage( object? Sender, MessageEventArgs E )
        {
            if ( !( Sender is TelegramBotClient ) ) throw new ApplicationException( "Ошибка при отправке сообщения." );

            var botClient = ( TelegramBotClient ) Sender;

            switch ( E.Message.Type )
            {
                case MessageType.Text:
                {
                    Log.Information( $"Text Message recieved in chat: " +
                                     $"{E.Message.Chat.Id} " +
                                     $"from: {E.Message.From.FirstName} " +
                                     $"{E.Message.From.LastName}. " +
                                     $"Text: {E.Message.Text}" );

                    IParser parser;
                    IBuilder builder;
                    IMusic  parsedMusic;

                    var uri = uriRegex.Match( E.Message.Text ).Value;

                    if ( !string.IsNullOrWhiteSpace( uri ) )
                        try
                        {
                            parser = _parsers.First( _ => E.Message.Text.Contains( _.Key ) ).Value;
                            parsedMusic = parser.ParseUri( E.Message.Text );
                            await botClient.SendTextMessageAsync( E.Message.Chat.Id, parsedMusic.ToString( ) );
                            builder = _builders.First( _ => !E.Message.Text.Contains( _.Key ) ).Value;;
                            string reply = builder.SearchMusic( parsedMusic ) ?? "Не получилось найти музыку";

                            await botClient.SendTextMessageAsync( E.Message.Chat.Id, reply );
                        }
                        catch ( InvalidOperationException )
                        {
                            Log.Error( "Uri received, but parser not found" );
                            await botClient.SendTextMessageAsync( E.Message.Chat.Id,
                                "Либо это не ссылка на музыку, либо я не умею работать с этим сервисом" );
                        }
                        catch ( NullReferenceException )
                        {
                            Log.Error( "Parser failed" );
                            await botClient.SendTextMessageAsync( E.Message.Chat.Id,
                                "Музыка не распознана" );
                        }

                    break;
                }

                case MessageType.VideoNote:
                {
                    Log.Information( "Videomessage received: " +
                                     $"{E.Message.Chat.Id} " +
                                     $"from: {E.Message.From.FirstName} " +
                                     $"{E.Message.From.LastName}.");
                    
                    var selectedStickerId = StickerIds.OrderBy( x => Guid.NewGuid( ) ).FirstOrDefault( );
                    await botClient.SendStickerAsync( E.Message.Chat.Id,
                        new InputOnlineFile( selectedStickerId ) );

                    break;
                }
            }
        }
    }
}