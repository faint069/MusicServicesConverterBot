using System.Linq;
using ConverterBot.Models;
using Yandex.Music.Api.Models.Album;
using Yandex.Music.Api.Models.Artist;
using Yandex.Music.Api.Models.Track;

namespace ConverterBot.Parsers
{
    public class YmParser : IParser
    {
        public IMusic ParseUri( string Uri )
        {
            //"https://music.yandex.ru/album/12360955/track/72099136"
            //https://music.yandex.ru/album/12343211
            //https://music.yandex.ru/artist/1810
            string[] uriParts = Uri.Split( "/" );
            
            if ( uriParts.Contains( "track" ) )
            {
                YTrack ymTrack = Clients.YandexMusicClient.Track.Get( Clients.YAusthStorage, 
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
                YAlbum ymAlbum = Clients.YandexMusicClient.Album.Get( Clients.YAusthStorage, 
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
                YArtistBriefInfo ymArtist = Clients.YandexMusicClient.Artist.Get( Clients.YAusthStorage, 
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
    }
}