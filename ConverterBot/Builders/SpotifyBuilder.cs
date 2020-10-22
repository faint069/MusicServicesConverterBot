using System.Collections.Generic;
using System.Linq;
using ConverterBot.Models;
using SpotifyAPI.Web;

namespace ConverterBot.Builders
{
    public class SpotifyBuilder : IBuilder
    {
        public string SearchMusic( IMusic musicToSearch )
        {
            switch ( musicToSearch )
            {
                case Track _:
                {
                    SearchResponse response = Clients.SpotifyClient.Search.Item(
                                                         new SearchRequest( SearchRequest.Types.Track,
                                                             musicToSearch.QueryString( ) ) )
                                                       .Result;
                    foreach ( FullTrack sTrack in response.Tracks.Items )
                    {
                        if ( musicToSearch.Equals( new Track( sTrack.Name,
                                                                    sTrack.Artists.First( ).Name,
                                                                    sTrack.Album.Name,
                                                                    0 ) ) )
                        {
                            return BuildUri( sTrack );
                        }
                    }

                    break;
                }
                case Album _:
                {
                    SearchResponse response = Clients.SpotifyClient.Search.Item(
                                                          new SearchRequest( SearchRequest.Types.Album,
                                                              musicToSearch.QueryString( ) ) )
                                                        .Result;
                    foreach ( SimpleAlbum sAlbum in response.Albums.Items )
                    {
                        if ( musicToSearch.Equals( new Album( sAlbum.Name,
                                                                    sAlbum.Artists.First( ).Name,
                                                                    sAlbum.ReleaseDate ) ) )
                        {
                            return BuildUri( sAlbum );
                        }
                    }

                    break;
                }
                case Artist artistToSearch:
                {
                    SearchResponse response = Clients.SpotifyClient.Search.Item(
                                                           new SearchRequest( SearchRequest.Types.Artist,
                                                               musicToSearch.QueryString( ) ) )
                                                          .Result;
                    foreach ( FullArtist sArtist in response.Artists.Items )
                    {
                        if ( sArtist.Name == artistToSearch.Name )
                        {
                            List<SimpleAlbum> sArtistAlbums = Clients.SpotifyClient.Artists.GetAlbums( sArtist.Id ).Result.Items;
                            foreach ( SimpleAlbum sSampleAlbum in sArtistAlbums )
                            {
                                if ( artistToSearch.SampleAlbum.Equals( new Album( sSampleAlbum.Name,
                                                                                         sSampleAlbum.Artists.First().Name,
                                                                                         sSampleAlbum.ReleaseDate ) ) )
                                {
                                    return BuildUri( sArtist );
                                }
                            }
                        }
                    }
                    break;
                }
            }

            return null;
        }

        private string BuildUri( FullTrack sTrack )
        {
            return string.Concat( @"https://open.spotify.com/track/", sTrack.Id );
        }

        private string BuildUri( SimpleAlbum sAlbum )
        {
            return string.Concat( @"https://open.spotify.com/album/", sAlbum.Id );
        }
        
        private string BuildUri( FullArtist sArtist )
        {
            return string.Concat( @"https://open.spotify.com/artist/", sArtist.Id );
        }
    }
}