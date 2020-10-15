namespace ConverterBot.Models
{
    public class Track : IMusic
    {
        private string _artist;
        private string _title;
        private string _album;
        private int    _trackIndex;
        private string _yandexId;
        private string _spotifyId;

        /// <summary>
        /// Пустой конструктор
        /// </summary>
        public Track( )
        {
            
        }
        
        /// <summary>
        /// Конструктор с параметрами
        /// </summary>
        /// <param name="title">Название трека</param>
        /// <param name="artist">Исполнитель</param>
        /// <param name="album">Альбом</param>
        /// <param name="trackIndex">Позиция в альбоме</param>
        /// <param name="spotifyId">ID трека в Спотифай</param>
        /// <param name="yandexId">ID трека в Яндекс Музыке</param>
        public Track( string title, string artist,  string album, int trackIndex, string spotifyId = null, string yandexId = null)
        {
            _title = title;
            _artist = artist;
            _album = album;
            _trackIndex = trackIndex;
            _spotifyId = spotifyId;
            _yandexId = yandexId;
        }

        public string Artist => _artist;

        public string Title => _title;

        public string Album => _album;
        
        public int TrackIndex => _trackIndex;

        public string YandexId => _yandexId;

        public string SpotifyId => _spotifyId;

        public override string ToString( )
        {
            return $"Track Title: {_title}\nAlbum: {_album}\nArtist: {_artist}";
        }
    }
}