using System;

namespace ConverterBot.Models
{
  public class Artist : IMusic
  {
    private Album _sampleAlbum;
    private string _name;
    private string _yandexId;
    private string _spotifyId;

    /// <summary>
    /// Пустой конструктор
    /// </summary>
    public Artist( )
    {
            
    }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="name">Наввание артиста</param>
    /// <param name="sampleAlbum">Альбом для примера</param>
    /// <param name="yandexId">ID Артиста в Яндекс Музыке</param>
    /// <param name="spotifyId">ID Артиста в Спотифай</param>
    public Artist( string name, Album sampleAlbum, string yandexId = null, string spotifyId = null )
    {
      _sampleAlbum = sampleAlbum;
      _yandexId = yandexId;
      _spotifyId = spotifyId;
      _name = name;
    }

    public string Name => _name;

    public Album SampleAlbum => _sampleAlbum;

    public string YandexId => _yandexId;

    public string SpotifyId => _spotifyId;

    public string QueryString( )
    {
      return $"{_name}";
    }

    public override string ToString( )
    {
      return $"Artist: {_name}\nSample Album:\n{_sampleAlbum.ToString( )}";
    }

    public bool Equals( IMusic other )
    {
      if ( !( other is Artist ) )
      {
        return false;
      }

      Artist otherArtist = ( Artist ) other;
      return string.Equals( _name, otherArtist.Name, StringComparison.OrdinalIgnoreCase) &&
             _sampleAlbum.Equals( otherArtist.SampleAlbum );
    }
  }
}