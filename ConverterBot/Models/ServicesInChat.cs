namespace ConverterBot.Models
{
  public class ServicesInChat
  {
    public ServicesInChat( )
    {
    }

    public ServicesInChat( long chatId, string[] services )
    {
      ChatId = chatId;
      Services = services;
    }

    public long ChatId { get; set; }
    
    public string[] Services { get; set; }
  }
}