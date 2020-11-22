using System;
using System.Collections.Generic;
using System.Linq;
using ConverterBot.Misc;
using ConverterBot.Models.Music;
using Serilog;
using Yandex.Music.Api;
using Yandex.Music.Api.Common;
using Yandex.Music.Api.Models.Album;
using Yandex.Music.Api.Models.Artist;
using Yandex.Music.Api.Models.Search;
using Yandex.Music.Api.Models.Search.Album;
using Yandex.Music.Api.Models.Search.Artist;
using Yandex.Music.Api.Models.Search.Track;
using Yandex.Music.Api.Models.Track;

namespace ConverterBot.Models.Clients
{
	public class YandexClient : IClient
	{
		private const string friendlyName = "Yandex Music";
		private const string name         = "music.yandex";
		
		private readonly YandexMusicApi yandexMusicClient;
		private readonly AuthStorage    yAuthStorage;

		public string FriendlyName => friendlyName;

		public string Name => name;

		public YandexClient( )
		{
			Log.Information( "Connecting to Yandex Music..." );

			yAuthStorage = new AuthStorage(  );
			yandexMusicClient = new YandexMusicApi( );
			yandexMusicClient.User.Authorize( yAuthStorage, Config.YMLogin, Config.YMPassword );
            
			Log.Information( "Connection to Yandex Music successful" );
		}

		public IMusic ParseUri( string uri )
		{
			//"https://music.yandex.ru/album/12360955/track/72099136"
			//https://music.yandex.ru/album/12343211
			//https://music.yandex.ru/artist/1810
			string[] uriParts = uri.Split( "/" );
                  
			if ( uriParts.Contains( "track" ) )
			{
				YTrack ymTrack = yandexMusicClient.Track.Get( yAuthStorage, 
																								uriParts.Last( ) )
																								.Result
																								.First();
				return new Track( ymTrack.Title, 
													ymTrack.Artists.First().Name, 
													ymTrack.Albums.First().Title, 
													0,
													null,
													ymTrack.Id);
			}
      
			if ( uriParts.Contains( "album" ) && !uriParts.Contains( "track" ) )
			{
				YAlbum ymAlbum = yandexMusicClient.Album.Get( yAuthStorage, 
						uriParts.Last( ) )
					.Result;
				return new Album( ymAlbum.Title, 
													ymAlbum.Artists.First( ).Name, 
													ymAlbum.Year.ToString( ),
													null,
													ymAlbum.Id);
			}
      
			if ( uriParts.Contains( "artist" ) )
			{
				YArtistBriefInfo ymArtist = yandexMusicClient.Artist.Get( yAuthStorage, 
																														uriParts.Last( ) )
																														.Result;
				YAlbum ymSampleAlbum = ymArtist.Albums.First();
				Album sampleAlbum = new Album( ymSampleAlbum.Title,
																			 ymArtist.Artist.Name,
																			 ymSampleAlbum.Year.ToString( ),
																			 null,
																			 ymSampleAlbum.Id);
				return new Artist( ymArtist.Artist.Name, 
													 sampleAlbum,
													 null,
													 uriParts.Last());
			}
      
			return null;
		}

		public string SearchMusic( IMusic musicToSearch )
		{
			switch ( musicToSearch )
      {
        case Track _:
        {
          YSearch response = yandexMusicClient.Search.Track( yAuthStorage,
              																							 musicToSearch.QueryString )
              																							 .Result;
          foreach ( YSearchTrackModel ymTrack in response.Tracks.Results )
          {
            if ( musicToSearch.Equals( new Track( ymTrack ) ) )
            {
              return BuildUri( ymTrack );
            }
          }

          int count = response.Tracks.Results.Count >= 5 ? 5 : response.Tracks.Results.Count;

          List<(Track, string)> tracksWithUris = response.Tracks.Results.Take( count )
                                                                        .Select( _ => ( new Track( _ ), BuildUri( _ ) ) )
                                                                        .ToList( );

          return InlineUriFormatter.FormatTracks( tracksWithUris );
        }
        
        case Album _:
        {
          YSearch response = yandexMusicClient.Search.Albums( yAuthStorage, 
            																									musicToSearch.QueryString )
            																									.Result;
          foreach ( YSearchAlbumModel ymAlbum in response.Albums.Results )
          {
            if ( musicToSearch.Equals( new Album( ymAlbum ) ) )
            {
              return BuildUri( ymAlbum );
            }
          }
          
          int count = response.Albums.Results.Count >= 5 ? 5 : response.Albums.Results.Count;

          List<(Album, string)> albumsWithUris = response.Albums.Results.Take( count )
                                                                        .Select( _ => ( new Album( _ ), BuildUri( _ ) ) )
                                                                        .ToList( );
          return InlineUriFormatter.FormatAlbums( albumsWithUris );
        }
        
        case Artist artistToSearch:
        {
          YSearch response = yandexMusicClient.Search.Artist( yAuthStorage, 
              																								musicToSearch.QueryString )
              																								.Result;
          foreach ( YSearchArtistModel ymArtist in response.Artists.Results )
          {
            if ( artistToSearch.Equals( new Artist(ymArtist.Name, null ) ) )
            {
              if ( artistToSearch.IsSampleAlbumEmpty )
              {
                return BuildUri( ymArtist );
              }
              else
              {
                var ymArtistBrief = yandexMusicClient.Artist.Get( yAuthStorage,
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
          }
          
          int count = response.Artists.Results.Count >= 5 ? 5 : response.Artists.Results.Count;

          List<(Artist, string)> artistsToSearch = response.Artists.Results.Take( count )
                                                                           .Select( _ => ( new Artist( _ ), BuildUri( _ ) ) )
                                                                           .ToList( );
          return InlineUriFormatter.FormatArtists( artistsToSearch );
        }
      }

      return null;
    }

    public string GetSearchUri( IMusic toSearch )
    {
      return string.Concat( "https://music.yandex.ru/search?text=", 
                            toSearch.QueryString.Replace( " ", "%20" ) );
    }

    private string BuildUri( YSearchTrackModel track )
    {
      //https://music.yandex.ru/album/3258239/track/3736259
      return string.Concat( @"https://music.yandex.ru/album/", track.Albums.First().Id, "/track/", track.Id  );
    }
    private string BuildUri( YSearchAlbumModel album )
    {
      //https://music.yandex.ru/album/3258239
      return string.Concat( @"https://music.yandex.ru/album/", album.Id  );
    }
    private string BuildUri( YSearchArtistModel artist )
    {
      //https://music.yandex.ru/artist/36800
      return string.Concat( @"https://music.yandex.ru/artist/", artist.Id  );
    }
	}
}