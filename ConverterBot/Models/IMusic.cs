namespace ConverterBot.Models
{
    public interface IMusic
    {
        string YandexId { get; }
        string SpotifyId { get; }

        public string ToString( );
    }
}