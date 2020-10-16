using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using ConverterBot.Models;
using SpotifyAPI.Web;

namespace ConverterBot.Parsers
{
    public class SpotifyParser : IParser
    {
        public IMusic ParseUri( string Uri )
        {
            //https://open.spotify.com/track/2fTdRdN73RgIgcUZN33dvt
            //https://open.spotify.com/album/2N367tN1eIXrHNVe86aVy4?si=A-1kS4F4Tfy2I_PYEDVhMA
            //https://open.spotify.com/artist/5cj0lLjcoR7YOSnhnX0Po5
            if ( Uri.Contains( "tospotify" ) )
            {
                string HTMLPattern = "(https://open.spotify.com).*?(\")";
                string URiPattern = @"(https://link\.tospotify\.com).*?($|\s)";
                Uri = Regex.Match( Uri, URiPattern ).Value.TrimEnd();
                WebClient wc = new WebClient(  );
                wc.Headers.Add ("user-agent", "MusicServicesConverterBot");
                string resp = wc.DownloadString( Uri );

                Uri = Regex.Match( resp, HTMLPattern ).Value.TrimEnd('"');
                if ( Uri.Contains( "?" ) )
                {
                    Uri = Uri.Split( "?" ).First( );
                }
            }
            
            string[] uriParts = Uri.Split( "/" );

            if ( uriParts.Contains( "track" ) )
            {
                FullTrack sTrack = Clients.SpotifyClient.Tracks.Get( uriParts.Last( ) ).Result;

                return new Track( sTrack.Name,
                                 sTrack.Artists.First( ).Name,
                                      sTrack.Album.Name,
                              0,
                               sTrack.Id );
            }

            if ( uriParts.Contains( "album" ) )
            {
                string id = "";
                if ( uriParts.Last( ).Contains( "?" ) )
                {
                    id = uriParts.Last( ).Split( "?" ).First( );
                }
                else
                {
                    id = uriParts.Last( );
                }

                FullAlbum sAlbum = Clients.SpotifyClient.Albums.Get( id ).Result;

                return new Album( sAlbum.Name,
                                 sAlbum.Artists.First( ).Name,
                                 sAlbum.ReleaseDate,
                              sAlbum.Id );
            }

            if ( uriParts.Contains( "artist" ) )
            {
                FullArtist sArtist = Clients.SpotifyClient.Artists.Get( uriParts.Last( ) ).Result;
                Paging<SimpleAlbum> sArtistAlbums = Clients.SpotifyClient.Artists.GetAlbums( uriParts.Last( ) ).Result;
                SimpleAlbum sSampleAlbum = sArtistAlbums.Items?.First( );

                Album sampleAlbum = new Album( sSampleAlbum?.Name,
                                                   sArtist.Name,
                                               sSampleAlbum?.ReleaseDate,
                                            sSampleAlbum?.Id );
 
                return new Artist( sArtist.Name,
                                   sampleAlbum,
                           sArtist.Id );
            }

            return null;
        }

    }
}
