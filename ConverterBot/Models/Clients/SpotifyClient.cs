using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using ConverterBot.Models.Music;
using Serilog;
using SpotifyAPI.Web;

namespace ConverterBot.Models.Clients
{
	public class SpotifyClient : IClient
	{
		private const string friendlyName = "Spotify";
		private const string name 			  = "spotify";

		private readonly SpotifyAPI.Web.SpotifyClient  _spotifyClient;

		public string FriendlyName => friendlyName;

		public string Name => name;

		public SpotifyClient( )
		{
			Log.Information( "Connecting to Spotify..." );

			SpotifyClientConfig spotifyConfig = SpotifyClientConfig.CreateDefault( )
																														 .WithAuthenticator(
																														 new ClientCredentialsAuthenticator(
																														   	 Config.SotifyClientID,
																														 		 Config.SotifyClientSecret ) );
			_spotifyClient = new SpotifyAPI.Web.SpotifyClient( spotifyConfig );
			
			Log.Information( "Connection to Spotify successful" );

		}

		public IMusic ParseUri( string uri )
		{
			//https://open.spotify.com/track/2fTdRdN73RgIgcUZN33dvt
      //https://open.spotify.com/album/2N367tN1eIXrHNVe86aVy4?si=A-1kS4F4Tfy2I_PYEDVhMA
      //https://open.spotify.com/artist/5cj0lLjcoR7YOSnhnX0Po5
      
      uri = NormalizeSpotifyUri( uri );
      
      string[] uriParts = uri.Split( "/" );

      if ( uriParts.Contains( "track" ) )
      {
        FullTrack sTrack = _spotifyClient.Tracks.Get( uriParts.Last( ) ).Result;

        return new Track( sTrack.Name,
          								sTrack.Artists.First( ).Name,
          							   sTrack.Album.Name,
          								0,
          								sTrack.Id );
      }

      if ( uriParts.Contains( "album" ) )
      {
        string id = uriParts.Last( ).Contains( "?" ) ? uriParts.Last( ).Split( "?" ).First( ) : uriParts.Last( );

        FullAlbum sAlbum = _spotifyClient.Albums.Get( id ).Result;

        return new Album( sAlbum.Name,
          							  sAlbum.Artists.First( ).Name,
          							  sAlbum.ReleaseDate,
          							  sAlbum.Id );
      }

      if ( uriParts.Contains( "artist" ) )
      {
        FullArtist sArtist = _spotifyClient.Artists.Get( uriParts.Last( ) ).Result;
        List<SimpleAlbum>? sArtistAlbums = _spotifyClient.Artists.GetAlbums( uriParts.Last( ) ).Result.Items;
        SimpleAlbum sSampleAlbum = sArtistAlbums?.First( );

        Album sampleAlbum = new Album( sSampleAlbum?.Name,
          														 sArtist.Name,
          														 sSampleAlbum?.ReleaseDate,
          														 sSampleAlbum?.Id );
 
        return new Artist( sArtist.Name,
          								 sampleAlbum,
          								 null,
          								 sArtist.Id );
      }

      return null;
		}

    public string SearchMusic( IMusic musicToSearch )
		{
			 switch ( musicToSearch )
      {
        case Track _:
        {
          SearchResponse response = _spotifyClient.Search.Item(
                                    new SearchRequest( SearchRequest.Types.Track,
                                        musicToSearch.QueryString ) )
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
          SearchResponse response = _spotifyClient.Search.Item(
              											new SearchRequest( SearchRequest.Types.Album,
              											    musicToSearch.QueryString ) )
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
          SearchResponse response = _spotifyClient.Search.Item(
              											new SearchRequest( SearchRequest.Types.Artist,
              											    musicToSearch.QueryString ) )
                                    .Result;
          foreach ( FullArtist sArtist in response.Artists.Items )
          {
            if ( sArtist.Name == artistToSearch.Name )
            {
              List<SimpleAlbum> sArtistAlbums = _spotifyClient.Artists.GetAlbums( sArtist.Id ).Result.Items;
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

    public string GetSearchUri( IMusic toSearch )
    {
      return string.Concat( "https://open.spotify.com/search/", 
                            toSearch.QueryString.Replace( " ", "%20" ) );
    }

    private string NormalizeSpotifyUri( string uri )
    {
      if ( uri.Contains( "tospotify" ) )
      {
        const string HTMLPattern = "(https://open.spotify.com).*?(\")";
        const string URiPattern = @"(https://link\.tospotify\.com).*?($|\s)";
        uri = Regex.Match( uri, URiPattern ).Value.TrimEnd();
        WebClient wc = new WebClient(  );
        wc.Headers.Add ("user-agent", "MusicServicesConverterBot");
        string resp = wc.DownloadString( uri );

        uri = Regex.Match( resp, HTMLPattern ).Value.TrimEnd('"');
      }
			
      if ( uri.Contains( "?" ) )
      {
        uri = uri.Split( "?" ).First( );
      }

      return uri;
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