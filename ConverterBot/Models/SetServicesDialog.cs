using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;

namespace ConverterBot.Models
{
    public class SetServicesDialog: IDialog
    {
        private readonly string[] _messages =
        {
            "Выберите первый сервис",
            "Выберите второй сервис",
            "Настройка завершена"
        };

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
                 return new InlineKeyboardMarkup( Clients.ClientsList.Except( SelectedServices )
                                                                     .Select( _ =>
                                                                              new InlineKeyboardButton
                                                                                    {Text = _, 
                                                                                    CallbackData = "set_command|" + _ } ) );
                }
            }
        }

        public SetServicesDialog( long chatId )
        {
            Step = 0;
            MaxSteps = 3;
            ChatId = chatId;
            IsOver = false;
            SelectedServices = new List<string>();
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