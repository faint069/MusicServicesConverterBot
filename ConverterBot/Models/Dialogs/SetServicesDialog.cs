using System.Collections.Generic;
using System.Linq;
using ConverterBot.Localization;
using Telegram.Bot.Types.ReplyMarkups;

namespace ConverterBot.Models.Dialogs
{
  public class SetServicesDialog: IDialog
  {
    private readonly string[] _messages = new string[3];

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
          return new InlineKeyboardMarkup( Services.Names.Except( SelectedServices )
            .Select( _ =>
              new InlineKeyboardButton
              {Text = _, 
                CallbackData = "set_command|" + _ } ) );
        }
      }
    }

    public SetServicesDialog( long chatId, string culture = "en" )
    {
      Step = 0;
      MaxSteps = 3;
      ChatId = chatId;
      IsOver = false;
      SelectedServices = new List<string>( );
      
      _messages[0] = Messages.SetServicesFirst.GetLocalized( culture );
      _messages[1] = Messages.SetServicesSecond.GetLocalized( culture );
      _messages[2] = Messages.SetServicesThird.GetLocalized( culture );

    }

    public long ChatId { get; }
    public int Step { get; set; }
    public bool IsOver { get; set; }
    public int MaxSteps { get; }

    public List<string> SelectedServices { get; }
        
    public void PerformStep( )
    {
      if ( !IsOver )
      {
        Bot.Bot.Client.SendTextMessageAsync( ChatId, _messages[Step], replyMarkup: Keyboard );
        Step++;
        if ( Step >= MaxSteps )
        {
          IsOver = true;
          DB.SetServicesForChat( ChatId, SelectedServices.ToArray(  ) );
        }
      }
    }
  }
}