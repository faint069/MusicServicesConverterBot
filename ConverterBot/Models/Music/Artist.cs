using System;
using System.Transactions;

namespace ConverterBot.Models.Music
{
  public class Artist : IMusic
  {
    private readonly Album  _sampleAlbum;
    private readonly string _name;
    private readonly string _yandexId;
    private readonly string _spotifyId;

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
      _name = name.Trim( );
    }

    public string Name => _name;

    public Album SampleAlbum => _sampleAlbum;

    public string YandexId => _yandexId;

    public string SpotifyId => _spotifyId;

    public string QueryString => $"{_name}";

    public override string ToString( )
    {
      return $"Artist: {_name}\nSample Album:\n{_sampleAlbum.ToString( )}";
    }

    public bool Equals( IMusic other )
    {
      if ( other is Artist otherArtist )
      {
        if ( otherArtist.SampleAlbum == null )
        {
          return string.Equals( _name, otherArtist.Name, StringComparison.OrdinalIgnoreCase );
        }

        return string.Equals( _name, otherArtist.Name, StringComparison.OrdinalIgnoreCase ) &&
               _sampleAlbum.Equals( otherArtist.SampleAlbum );
      }

      return false;
    }
  }
}