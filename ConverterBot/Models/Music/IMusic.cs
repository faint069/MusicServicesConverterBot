namespace ConverterBot.Models.Music
{
  public interface IMusic
  {
    string YandexId { get; }
    string SpotifyId { get; }

    public string QueryString( );
    public string ToString( );
    public bool Equals( IMusic other );
  }
}