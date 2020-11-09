namespace ConverterBot.Models
{
  public class ServicesInChat
  {
    public ServicesInChat( )
    {
    }

    public long ChatId { get; set; }
    public string[] Services { get; set; }

    public ServicesInChat( long chatId, string[] services )
    {
      ChatId = chatId;
      Services = services;
    }
  }
}