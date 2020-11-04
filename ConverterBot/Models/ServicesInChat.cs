namespace ConverterBot.Models
{
  public class ServicesInChat
  {
    public long ChatId { get; }
    public string[] Services { get; }

    public ServicesInChat( long chatId, string[] services )
    {
      ChatId = chatId;
      Services = services;
    }
  }
}