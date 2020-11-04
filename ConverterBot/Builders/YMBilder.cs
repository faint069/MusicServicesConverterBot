using System;
using System.Linq;
using ConverterBot.Models;
using Yandex.Music.Api.Models.Album;
using Yandex.Music.Api.Models.Search;
using Yandex.Music.Api.Models.Search.Album;
using Yandex.Music.Api.Models.Search.Artist;
using Yandex.Music.Api.Models.Search.Track;

namespace ConverterBot.Builders
{
  public class YmBuilder : IBuilder
  {
    public string SearchMusic( IMusic musicToSearch )
    {
      switch ( musicToSearch )
      {
        case Track _:
        {
          YSearch response = Clients.YandexMusicClient.Search.Track( Clients.YAusthStorage, 
              musicToSearch.QueryString( ) )
            .Result;
          foreach ( YSearchTrackModel ymTrack in response.Tracks.Results )
          {
            if ( musicToSearch.Equals( new Track( ymTrack.Title,
              ymTrack.Artists.First( ).Name,
              ymTrack.Albums.First( ).Title,
              0 ) ) )
            {
              return BuildUri( ymTrack );
            }
                        
          }
          break;
        }
        case Album _:
        {
          YSearch response = Clients.YandexMusicClient.Search.Albums( Clients.YAusthStorage, 
              musicToSearch.QueryString( ) )
            .Result;
          foreach ( YSearchAlbumModel ymAlbum in response.Albums.Results )
          {
            if ( musicToSearch.Equals( new Album( ymAlbum.Title,
              ymAlbum.Artists.First( ).Name,
              ymAlbum.Year.ToString( )) ) )
            {
              return BuildUri( ymAlbum );
            }
                        
          }
          break;
        }
        case Artist artistToSearch:
        {
          YSearch response = Clients.YandexMusicClient.Search.Artist( Clients.YAusthStorage, 
              musicToSearch.QueryString( ) )
            .Result;
          foreach ( YSearchArtistModel ymArtist in response.Artists.Results )
          {
            if (string.Equals( artistToSearch.Name, ymArtist.Name, StringComparison.OrdinalIgnoreCase ) )
            {
              var ymArtistBrief = Clients.YandexMusicClient.Artist.Get( Clients.YAusthStorage,
                  ymArtist.Id )
                .Result;
              foreach ( YAlbum ymAlbum in ymArtistBrief.Albums )
              {
                if ( artistToSearch.SampleAlbum.Equals( new Album( ymAlbum.Title, 
                  ymArtist.Name,
                  ymAlbum.Year.ToString( )) ) )
                {
                  return BuildUri( ymArtist );
                }
              }
            }
          }
                        
          break;
        }
      }

      return null;
    }

    private string BuildUri( YSearchTrackModel track )
    {
      //https://music.yandex.ru/album/3258239/track/3736259
      return string.Concat( @"https://music.yandex.ru/album/" , track.Albums.First().Id, "/track/", track.Id  );
    }
    private string BuildUri( YSearchAlbumModel album )
    {
      //https://music.yandex.ru/album/3258239
      return string.Concat( @"https://music.yandex.ru/album/" , album.Id  );
    }
    private string BuildUri( YSearchArtistModel artist )
    {
      //https://music.yandex.ru/artist/36800
      return string.Concat( @"https://music.yandex.ru/artist/" , artist.Id  );
    }
  }
}