using System.Linq;
using ConverterBot.Models;
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
                    var response = Clients.YandexMusicClient.Search.Track( Clients.YAusthStorage, 
                                                                         musicToSearch.QueryString( ) )
                                                                                  .Result;
                    foreach ( var ymTrack in response.Tracks.Results )
                    {
                        musicToSearch.Equals( new Track( ymTrack.Title,
                                                         ymTrack.Artists.First( ).Name,
                                                        ymTrack.Albums.First( ).Title,
                                                     0 ) );
                        return BuildUri( ymTrack.Id, ymTrack.Albums.First().Id );
                    }
                    break;
                }
                case Album _:
                {
                    
                    break;
                }
                case Artist _:
                {

                    break;
                }
            }

            return null;
        }

        private string BuildUri( string trackId, string albumId )
        {
            //https://music.yandex.ru/album/3258239/track/3736259
            return string.Concat( "https://music.yandex.ru/alum/" , albumId, "/track", trackId  );
        }
    }
}