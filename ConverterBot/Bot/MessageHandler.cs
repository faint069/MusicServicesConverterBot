﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConverterBot.Localization;
using ConverterBot.Misc;
using ConverterBot.Models;
using ConverterBot.Models.Clients;
using ConverterBot.Models.Dialogs;
using ConverterBot.Models.Music;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ConverterBot.Bot
{
  public static class MessageHandler
  {
    private static readonly Dictionary<long, SetServicesDialog> Dialogs = 
                                new Dictionary<long, SetServicesDialog>();

    static MessageHandler( )
    {
    }
       
    public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
      if (update.Message is Message message)
      {
        switch ( message.Type )
        {
          case MessageType.Text:
            HandleText( message );
            break;
          case MessageType.VideoNote:
            HandleVideonote( message );
            break;
          case MessageType.Sticker:
            HandleSticker( message );
            break;
          default: 
            HandleCommon( message );
            break;
        }
      }

      if ( update.CallbackQuery != null )
      {
        HandeCallbackQuery( update.CallbackQuery );
      }
    }
    
    private static void HandleCommon( Message message )
    {
      Log.Warning( "Unknown Message received in chat: " +
                   $"{message.Chat.Id} " +
                   $"from: {message.From.FirstName} " +
                   $"{message.From.LastName}. " +
                   $"Text: {message.Text} " + 
                   $"Type: {message.Type} " );

      //Bot.Client.SendTextMessageAsync( message.Chat.Id, 
      //                                 Messages.CantProcess.GetLocalized( message.From.LanguageCode ) );
    }

    private static void HandleSticker( Message message )
    {
      Log.Warning( "Sticker Message received in chat: " +
                   $"{message.Chat.Id} "                +
                   $"from: {message.From.FirstName} "   +
                   $"{message.From.LastName}. "         +
                   $"Sticker ID: {message.Sticker.FileId} " );
    }

    private static void HandleText( Message message )
    {
      Log.Information( "Text Message received in chat: " +
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
              ProcessCommand( message.EntityValues.ElementAt( i ), message );
              break;
            case MessageEntityType.Url:
              ProcessUri( message.EntityValues.ElementAt( i ), message );
              break;
            default:
              //Bot.Client.SendTextMessageAsync( message.Chat.Id,
              //                                 Messages.CantProcess.GetLocalized( message.From.LanguageCode ));
              break;
          }
        }
      }
    }

    private static void HandleVideonote( Message message )
    {
      Log.Information( "Video Message received: "         +
                       $"{message.Chat.Id} "              +
                       $"from: {message.From.FirstName} " +
                       $"{message.From.LastName}." );

      
      if ( Stickers.Count != 0 )
      {
        Bot.Client.SendStickerAsync( message.Chat.Id,
                                     Stickers.GetRandomSmockingBotSticker(  ) );
      }
    }

    private static void ProcessUri( string uri, Message message )
    {
      if ( Services.TryGetClientForUri( uri, out IClient inClient ) )
      {
        string[] servicesInChat = DB.GetServicesForChat( message.Chat.Id );
        if ( servicesInChat != null )
        {
          if ( servicesInChat.Contains( inClient.FriendlyName ) )
          {
            IMusic parsedMusic = inClient.ParseUri( uri );
            if ( parsedMusic != null )
            {
              Bot.Client.SendTextMessageAsync( message.Chat.Id, parsedMusic.ToString( ) );
              
              var    servicesToSearch = servicesInChat.Where( _ => _ != inClient.FriendlyName );
              
              var reply = string.Empty;
              
              foreach ( var service in servicesToSearch )
              {
                IClient outClient = Services.GetClientFromFriendlyName( service );
                reply += $"{outClient.FriendlyName}:{Environment.NewLine}";
                reply += outClient.SearchMusic( parsedMusic ) ??
                               Messages.MusicNotFound.GetLocalized( message.From.LanguageCode ) + 
                               outClient.GetSearchUri( parsedMusic );

                reply += Environment.NewLine + Environment.NewLine;
              }

              Bot.Client.SendTextMessageAsync( message.Chat.Id, reply, ParseMode.Markdown, disableWebPagePreview: !reply.IsUri(  ));
            }
            else
            {
              //Bot.Client.SendTextMessageAsync( message.Chat.Id,
              //                                 Messages.WrongUri.GetLocalized(message.From.LanguageCode) );
            }
          }
          else
          {
            Bot.Client.SendTextMessageAsync( message.Chat.Id,
                                             Messages.ServiceINonInChat.GetLocalized( message.From.LanguageCode ) );
          }
        }
        else
        {
          Bot.Client.SendTextMessageAsync( message.Chat.Id, 
                                           Messages.NoServicesInChat.GetLocalized( message.From.LanguageCode ) );
        }
      }
      else
      {
        Bot.Client.SendTextMessageAsync( message.Chat.Id, 
                                         Messages.WrongUri.GetLocalized(message.From.LanguageCode) );
      }
    }

    private static void ProcessCommand( string command, Message message )
    {
      if ( command.StartsWith( "/start" ) )
      {
        Bot.Client.SendTextMessageAsync( message.Chat.Id, Messages.Greetings
          .GetLocalized( message.From.LanguageCode ) );
      }
      else if ( command.StartsWith( "/help" ) )
      {
        Bot.Client.SendTextMessageAsync( message.Chat.Id,
          Messages.Help.GetLocalized( message.From.LanguageCode ) );
      }
      else if ( command.StartsWith( "/set_services") )
      {
        if ( Dialogs.ContainsKey( message.Chat.Id ) )
        {
          Dialogs.Remove( message.Chat.Id );
        }

        var dialog = new SetServicesDialog( message.Chat.Id, message.From.LanguageCode );

        dialog.PerformStep( );

        Dialogs.TryAdd( message.Chat.Id, dialog );
      }
      else if ( command.StartsWith( "/get_services" ) )
      {
        var services = DB.GetServicesForChat( message.Chat.Id );
        if ( services != null )
        {
          Bot.Client.SendTextMessageAsync( message.Chat.Id, Messages.ServicesInChat
                                                                    .GetLocalized( message.From.LanguageCode ) +
                                                            string.Join( ", ", services ) );
        }
        else
        {
          Bot.Client.SendTextMessageAsync( message.Chat.Id, Messages.NoServicesInChat
            .GetLocalized( message.From.LanguageCode ) );
        }
      }
    }

    private static void HandeCallbackQuery( CallbackQuery query )
    {
      if ( query.Data != null && query.Message != null )
      {
        if ( query.Data.StartsWith( "set_command" ) )
        {
          string callbackData = query.Data.Split( '|' ).Last( );

          long chatId = query.Message.Chat.Id;

          if ( Dialogs.TryGetValue( chatId, out SetServicesDialog dialog ) )
          {
            if ( callbackData == "done" )
            {
              if ( dialog.SelectedServices.Count >= 2 )
              {
                dialog.IsOver = true;
              }
              else
              {
                Bot.Client.SendTextMessageAsync( chatId, Messages.NotEnoughServices
                                                                 .GetLocalized( dialog.Culture ) );
                return;
              }
            }
            else
            {
              dialog.SelectedServices.Add( callbackData );
            }

            dialog.PerformStep( );
            if ( dialog.IsOver )
            {
              Dialogs.Remove( chatId );
            }
          }
        }
      }
    }
  }
}