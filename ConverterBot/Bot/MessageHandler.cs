using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ConverterBot.Builders;
using ConverterBot.Misc;
using ConverterBot.Models;
using ConverterBot.Parsers;
using Serilog;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace ConverterBot.Bot
{
    public static class MessageHandler
    {
        private static readonly Regex UriRegex = new Regex(@"((([A-Za-z]{3,9}:(?:\/\/)?)(?:[-;:&=\+\$,\w]+@)?[A-Za-z0-9.-]+|(?:www.|[-;:&=\+\$,\w]+@)[A-Za-z0-9.-]+)((?:\/[\+~%\/.\w-_]*)?\??(?:[-\+=&;%@.\w_]*)#?(?:[.\!\/\\w]*))?)");

        private static Dictionary<string, IParser> _parsers = new Dictionary<string, IParser>
        {
            {Services.YandexMusic, new YmParser( )},
            {Services.Spotify,     new SpotifyParser( )}
        };
            
        private static Dictionary<string, IBuilder> _builders = new Dictionary<string, IBuilder>
        {
            {Services.YandexMusic,  new YmBuilder(  )},
            {Services.Spotify,      new SpotifyBuilder(  )},
            {Services.YoutubeMusic, new YoutubeMusicBuilder(  )}
        };
        
        public static void BotOnMessage( object? sender, MessageEventArgs e )
        { 
            _ = e.Message.Type switch
            {
                MessageType.Text => HandleText( e.Message ),
                MessageType.VideoNote => HandleVideonote( e.Message ),
                _ => HandleCommon( e.Message )
            };
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

        public static void BotOnInlineQuery( object? sender, CallbackQueryEventArgs callbackQueryEventArgs )
        {
            var a = callbackQueryEventArgs;
        }

        private static string HandleCommon( Message eMessage )
        {
            throw new NotImplementedException( );
        }
        
        private static string HandleText( Message message )
        {
            Log.Information( $"Text Message recieved in chat: " +
                             $"{message.Chat.Id} " +
                             $"from: {message.From.FirstName} " +
                             $"{message.From.LastName}. " +
                             $"Text: {message.Text}" );

            if ( message.Text.StartsWith( '/' ) )
            {
                Bot.Client.SendTextMessageAsync( message.Chat.Id, "Выберите первый сервис",
                    replyMarkup: ProcessCommand( message.Text ) );
                return null;
            }

            var uri = UriRegex.Match( message.Text ).Value;

            if ( !string.IsNullOrWhiteSpace( uri ) )
            {
                try
                {
                    IParser parser = _parsers.First( _ => message.Text.Contains( _.Key ) ).Value;
                    IMusic parsedMusic = parser.ParseUri( message.Text );
                    Bot.Client.SendTextMessageAsync( message.Chat.Id, parsedMusic.ToString( ) );
                    IBuilder builder = _builders.First( _ => !message.Text.Contains( _.Key ) ).Value;
                    string reply = builder.SearchMusic( parsedMusic ) ?? "Не получилось найти музыку";

                    Bot.Client.SendTextMessageAsync( message.Chat.Id, reply );
                }
                catch ( InvalidOperationException )
                {
                    Log.Error( "Uri received, but parser not found" );
                    Bot.Client.SendTextMessageAsync( message.Chat.Id,
                        "Либо это не ссылка на музыку, либо я не умею работать с этим сервисом" );
                }
                catch ( NullReferenceException )
                {
                    Log.Error( "Parser failed" );
                    Bot.Client.SendTextMessageAsync( message.Chat.Id,
                        "Музыка не распознана" );

                }
            }

            return null;
        }

        private static string HandleVideonote( Message message )
        {
            Log.Information( "Videomessage received: " +
                                      $"{message.Chat.Id} " +
                                      $"from: {message.From.FirstName} " +
                                      $"{message.From.LastName}.");
                     
            Bot.Client.SendStickerAsync( message.Chat.Id,
                                        new InputOnlineFile( Stickers.GetRandomSmickingBotSticker(  ) ) );
            return null;
        }
    }
}
