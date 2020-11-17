using System.Collections.Generic;
using System.Linq;
using ConverterBot.Localization;
using ConverterBot.Misc;
using ConverterBot.Models;
using ConverterBot.Models.Clients;
using ConverterBot.Models.Dialogs;
using ConverterBot.Models.Music;
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
    private static readonly Dictionary<long, SetServicesDialog> Dialogs = 
                                new Dictionary<long, SetServicesDialog>();

    static MessageHandler( )
    {
    }
       
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

      Bot.Client.SendTextMessageAsync( message.Chat.Id, 
                                       Messages.CantProcess.GetLocalized( message.From.LanguageCode ) );
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
              Bot.Client.SendTextMessageAsync( message.Chat.Id,
                                               Messages.CantProcess.GetLocalized( message.From.LanguageCode ));
              break;
          }
        }
      }
    }

    private static void HandleVideonote( Message message )
    {
      Log.Information( "Video Message received: " +
                       $"{message.Chat.Id} " +
                       $"from: {message.From.FirstName} " +
                       $"{message.From.LastName}.");
                     
      Bot.Client.SendStickerAsync( message.Chat.Id,
        new InputOnlineFile( Stickers.GetRandomSmickingBotSticker(  ) ) );
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
              IClient outClient =
                Services.GetClientFromFriendlyName( servicesInChat.Single( _ => _ != inClient.FriendlyName ) );
              string reply = outClient.SearchMusic( parsedMusic ) ??
                             Messages.MusicNotFound.GetLocalized( message.From.LanguageCode ) + 
                             outClient.GetSearchUri( parsedMusic );

              Bot.Client.SendTextMessageAsync( message.Chat.Id, reply );
            }
            else
            {
              Bot.Client.SendTextMessageAsync( message.Chat.Id,
                                               Messages.WrongUri.GetLocalized(message.From.LanguageCode) );
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
      switch ( command )
      {
        case "/start":
          Bot.Client.SendTextMessageAsync( message.Chat.Id, Messages.Greetings
                                                                 .GetLocalized( message.From.LanguageCode ));
          break;
        case "/help":
          Bot.Client.SendTextMessageAsync( message.Chat.Id, 
                                           Messages.Help.GetLocalized( message.From.LanguageCode ));
          break;
        case "/set_services":
        {
          if ( Dialogs.ContainsKey( message.Chat.Id ) )
          {
            Dialogs.Remove( message.Chat.Id );
          }

          var dialog = new SetServicesDialog( message.Chat.Id, message.From.LanguageCode );
                
          dialog.PerformStep( );

          Dialogs.TryAdd( message.Chat.Id, dialog );
          break;
        }
        case "/get_services":
        {
          var services = DB.GetServicesForChat( message.Chat.Id );
          if ( services != null )
          {
            Bot.Client.SendTextMessageAsync( message.Chat.Id, Messages.ServicesInChat
                                                                           .GetLocalized( message.From.LanguageCode )
                                                                           + $" {services[0]}, {services[1]}" );
          }
          else
          {
            Bot.Client.SendTextMessageAsync( message.Chat.Id, Messages.NoServicesInChat
                                                                           .GetLocalized( message.From.LanguageCode ) );
          }
          break;
        }
      }
    }
    public static void BotOnInlineQuery( object? sender, CallbackQueryEventArgs callbackQueryEventArgs )
    {
      if ( callbackQueryEventArgs.CallbackQuery.Data.StartsWith( "set_command" ) )
      {
        long chatId = callbackQueryEventArgs.CallbackQuery.Message.Chat.Id;
        
        Bot.Client.EditMessageReplyMarkupAsync( new ChatId( chatId ),
                                                callbackQueryEventArgs.CallbackQuery.Message.MessageId, 
                                                InlineKeyboardMarkup.Empty( ) );
        
        string service = callbackQueryEventArgs.CallbackQuery.Data.Split( '|' ).Last( );
        if ( Dialogs.TryGetValue( chatId, out SetServicesDialog dialog ) )
        {
          dialog.SelectedServices.Add( service );
          dialog.PerformStep(  );
          if ( dialog.IsOver )
          {
            Dialogs.Remove( chatId );
          }
        }
      }
    }
  }
}