using System;
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
    //private static readonly Dictionary<string, IClient> Clients = new Dictionary<string, IClient>( );
    private static Dictionary<long, SetServicesDialog> _dialogs = new Dictionary<long, SetServicesDialog>();

    static MessageHandler( )
    {
      //YandexClient  yandexMC  = new YandexClient(  );
      //SpotifyClient SC        = new SpotifyClient(  );
      //YoutubeClient youtubeMC = new YoutubeClient( );
      //  
      //Clients.Add( yandexMC.Name,  yandexMC );
      //Clients.Add( SC.Name,        SC );
      //Clients.Add( youtubeMC.Name, youtubeMC );
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
      };
    }
        

    private static void HandleCommon( Message message )
    {
      Log.Warning( $"Unknown Message recieved in chat: " +
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
      Log.Information( "Videomessage received: " +
                       $"{message.Chat.Id} " +
                       $"from: {message.From.FirstName} " +
                       $"{message.From.LastName}.");
                     
      Bot.Client.SendStickerAsync( message.Chat.Id,
        new InputOnlineFile( Stickers.GetRandomSmickingBotSticker(  ) ) );
    }
        
    private static void ProcessUri( string uri, Message message )
    {
      if ( Services.TryGetClientForUri( uri, out IClient client ) )
      {
        try
        {
          IMusic parsedMusic = client.ParseUri( uri );
          Bot.Client.SendTextMessageAsync( message.Chat.Id, parsedMusic.ToString( ) );
          string reply = client.SearchMusic( parsedMusic ) ?? 
                         Messages.MusicNotFound.GetLocalized( message.From.LanguageCode );

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
          if ( _dialogs.ContainsKey( message.Chat.Id ) )
          {
            _dialogs.Remove( message.Chat.Id );
          }

          var dialog = new SetServicesDialog( message.Chat.Id, message.From.LanguageCode );
                
          dialog.PerformStep( );

          _dialogs.TryAdd( message.Chat.Id, dialog );
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
        SetServicesDialog dialog;
        if ( _dialogs.TryGetValue( chatId, out dialog ) )
        {
          dialog.SelectedServices.Add( service );
          dialog.PerformStep(  );
          if ( dialog.IsOver )
          {
            _dialogs.Remove( chatId );
          }
        }
      }
    }
  }
}