﻿using System;
using System.Linq;

namespace ConverterBot.Models.Music
{
  public class Album : IMusic
  {
    private readonly string _title;
    private readonly string _artist;
    private readonly string _year;
    private readonly string _yandexId;
    private readonly string _spotifyId;

    /// <summary>
    /// Пустой конструктор
    /// </summary>
    public Album( )
    {
    }

    /// <summary>
    /// Коснтруктор с параметрами
    /// </summary>
    /// <param name="title">Название альбома</param>
    /// <param name="artist">Исполнитель</param>
    /// <param name="year">Год выпуска</param>
    /// <param name="yandexId">ID альбома в Яндекс Музыке</param>
    /// <param name="spotifyId">ID альбома в Спотифай</param>
    public Album( string title, string artist, string year, string yandexId = null, string spotifyId = null )
    {
      _title     = title.Trim( );
      _artist    = artist.Trim( );
      _yandexId  = yandexId;
      _spotifyId = spotifyId;

      //Если пришла полная дата, оставляем только год
      if ( year.Length <= 4 )
      {
        _year = year;
      }
      else
      {
        char separator = year.ToCharArray( ).First( _ => !char.IsDigit( _ ) );
        _year = year.Split( separator ).First( _ => _.Length == 4 );
      }
    }

    public string Title  => _title;
      
    public string Artist => _artist;
           
    public string Year   => _year;

    public string YandexId => _yandexId;

    public string SpotifyId => _spotifyId;

    /// <summary>
    /// Строка для поиска
    /// </summary>
    /// <returns></returns>
    public string QueryString => $"{_title}";

    public override string ToString( )
    {
      return $"Album Title: {_title}\nAlbum Artist: {_artist}\nRelease Year: {_year}";
    }

    public bool Equals( IMusic other )
    {
      if ( other is Album otherAlbum )
      {
        return ( _title.Contains( otherAlbum.Title, StringComparison.OrdinalIgnoreCase ) ||
                 otherAlbum.Title.Contains( _title, StringComparison.OrdinalIgnoreCase) ) &&
                 string.Equals( _artist, otherAlbum.Artist, StringComparison.OrdinalIgnoreCase );
      }
      
      return false;
    }
  }
}