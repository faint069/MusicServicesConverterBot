﻿using System;
using System.Collections.Generic;
using System.Linq;
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
        private static readonly Dictionary<string, IParser> Parsers = new Dictionary<string, IParser>
        {
            {Services.YandexMusic, new YmParser( )},
            {Services.Spotify,     new SpotifyParser( )}
        };
            
        private static readonly Dictionary<string, IBuilder> Builders = new Dictionary<string, IBuilder>
        {
            {Services.YandexMusic,  new YmBuilder(  )},
            {Services.Spotify,      new SpotifyBuilder(  )},
            {Services.YoutubeMusic, new YoutubeMusicBuilder(  )}
        };

        public static void BotOnMessage( object? sender, MessageEventArgs e )
        { 
            switch ( e.Message.Type )
            {
                case MessageType.Text:
                    HandleText( e.Message );
                    break;
                case MessageType.VideoNote:
                    HandleVideonote( e.Message );
                    break;
                default: 
                    HandleCommon( e.Message );
                    break;
            };
        }
        
        public static void BotOnInlineQuery( object? sender, CallbackQueryEventArgs callbackQueryEventArgs )
        {
            var a = callbackQueryEventArgs;
        }

        private static void HandleCommon( Message message )
        {
            Log.Warning( $"Unknown Message recieved in chat: " +
                          $"{message.Chat.Id} " +
                          $"from: {message.From.FirstName} " +
                          $"{message.From.LastName}. " +
                          $"Text: {message.Text} " + 
                          $"Type: {message.Type} " );
        }
        
        private static void HandleText( Message message )
        {
            Log.Information( $"Text Message recieved in chat: " +
                             $"{message.Chat.Id} " +
                             $"from: {message.From.FirstName} " +
                             $"{message.From.LastName}. " +
                             $"Text: {message.Text}" );

            if ( message.Entities!= null && message.Entities.Any( ))
            {
                for ( var i = 0; i < message.Entities.Length; i++ )
                {
                    switch ( message.Entities[i].Type )
                    {
                        case MessageEntityType.BotCommand:
                            ProcessCommand( message.EntityValues.ElementAt( i ) );
                            break;
                        case MessageEntityType.Url:
                            ProcessUri( message.EntityValues.ElementAt( i ), message.Chat.Id );
                            break;
                        default:
                            Bot.Client.SendTextMessageAsync( message.Chat.Id, "Я не умею с этим работать" );
                            break;
                    }
                }
            }
        }

        private static void HandleVideonote( Message message )
        {
            Log.Information( "Videomessage received: " +
                                      $"{message.Chat.Id} " +
                                      $"from: {message.From.FirstName} " +
                                      $"{message.From.LastName}.");
                     
            Bot.Client.SendStickerAsync( message.Chat.Id,
                                        new InputOnlineFile( Stickers.GetRandomSmickingBotSticker(  ) ) );
        }
        

        private static void ProcessUri( string uri, long chatId )
        {
            if ( !string.IsNullOrWhiteSpace( uri ) )
            {
                try
                {
                    IParser parser = Parsers.First( _ => uri.Contains( _.Key ) ).Value;
                    IMusic parsedMusic = parser.ParseUri( uri );
                    Bot.Client.SendTextMessageAsync( chatId, parsedMusic.ToString( ) );
                    IBuilder builder = Builders.First( _ => !uri.Contains( _.Key ) ).Value;
                    string reply = builder.SearchMusic( parsedMusic ) ?? "Не получилось найти музыку";

                    Bot.Client.SendTextMessageAsync( chatId, reply );
                }
                catch ( InvalidOperationException )
                {
                    Log.Error( "Uri received, but parser not found" );
                    Bot.Client.SendTextMessageAsync( chatId,
                        "Либо это не ссылка на музыку, либо я не умею работать с этим сервисом" );
                }
                catch ( NullReferenceException )
                {
                    Log.Error( "Parser failed" );
                    Bot.Client.SendTextMessageAsync( chatId,
                        "Музыка не распознана" );

                }
            }
        }
        private static void ProcessCommand( string command )
        {
            if ( command == "/set_services" )
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
            }
        }
    }
}
