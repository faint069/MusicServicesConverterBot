using System.Collections.Generic;
using System.Linq;
using ConverterBot.Localization;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ConverterBot.Models.Dialogs
{
  public class SetServicesDialog: IDialog
  {
    private readonly string[] _messages = new string[3];
    private readonly string   _culture;
    private          Message  _lastMessage;

    private InlineKeyboardMarkup Keyboard
    {
      get
      {
        if ( Step == MaxSteps - 1 )
        {
          return InlineKeyboardMarkup.Empty( );
        }
        else
        {
          List<InlineKeyboardButton> buttons1Row = Services.FriendlyNames.Except( SelectedServices )
                                                                         .Select( _ => new InlineKeyboardButton
                                                                                       {
                                                                                         Text = _,
                                                                                         CallbackData = "set_command|" + _
                                                                                       } )
                                                                         .ToList( );
          
          List<InlineKeyboardButton> buttons2Row = new List<InlineKeyboardButton>
                                                   {
                                                     new InlineKeyboardButton
                                                     {
                                                       Text = "Done",
                                                       CallbackData = "set_command|done"
                                                     } 
                                                   };
          
        return new InlineKeyboardMarkup( new List<List<InlineKeyboardButton>>{ buttons1Row, buttons2Row } );
        }
      }
    }

    public SetServicesDialog( long chatId, string culture = "en" )
    {
      Step = 0;
      MaxSteps = Services.Count + 1;
      ChatId = chatId;
      _culture = culture;
      _lastMessage = new Message( );
      IsOver = false;
      SelectedServices = new List<string>( );
      
      _messages[0] = Messages.SetServicesFirst.GetLocalized( culture );
    }

    public long ChatId { get; }
    public int Step { get; set; }
    public bool IsOver { get; set; }
    public int MaxSteps { get; }
    public string Culture => _culture;

    public List<string> SelectedServices { get; }
        
    public void PerformStep( )
    {
      if ( !IsOver )
      {
        if ( Step == 0 )
        {
          _lastMessage = Bot.Bot.Client.SendTextMessageAsync( ChatId, _messages[Step], replyMarkup: Keyboard ).Result;
        }

        if ( Step >= 1 && Step < MaxSteps )
        {
          _lastMessage = Bot.Bot.Client.EditMessageReplyMarkupAsync( ChatId, _lastMessage.MessageId, Keyboard ).Result;
        }

        Step++;
        if ( Step >= MaxSteps )
        {
         IsOver = true;
         FinishDialog(  );
        }
      }
      
      else
      {
        FinishDialog(  );
      }
    }

    private void FinishDialog( )
    {
      DB.SetServicesForChat( ChatId, SelectedServices.ToArray(  ) );
      Bot.Bot.Client.EditMessageReplyMarkupAsync( ChatId, _lastMessage.MessageId, InlineKeyboardMarkup.Empty( ) );
      Bot.Bot.Client.SendTextMessageAsync( ChatId, Messages.ServicesInChat.GetLocalized( _culture ) + 
                                                   string.Join( ", ", SelectedServices ) );
    }
  }
}