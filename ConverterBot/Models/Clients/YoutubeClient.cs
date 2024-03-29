﻿using System.Linq;
using ConverterBot.Models.Music;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Serilog;

namespace ConverterBot.Models.Clients
{
	public class YoutubeClient : IClient
	{
		private const string friendlyName = "YouTube Music";
		private const string name = "music.youtube";

    private readonly YouTubeService _youtubeService;
		public string FriendlyName => friendlyName;

		public string Name => name;

    public YoutubeClient( )
    {
      Log.Information( "Connecting to Spotify..." );

      _youtubeService = new YouTubeService( new BaseClientService.Initializer
      {
        ApiKey = Config.YoutubeApiKey,
        ApplicationName = "MusicServicesConverterBot"
      } );
      
      Log.Information( "Connection to Spotify successful" );
    }

		public IMusic ParseUri( string uri )
    {
	    //https://music.youtube.com/watch?v=TNMANPoUWrk&feature=share
	    //https://music.youtube.com/playlist?list=OLAK5uy_nmfIy_KLAysXSPgi-aoi5zQ9dVoEKvzss
	    //https://music.youtube.com/channel/UCpqq5sZCFH6vkIrO9PZc8bA
	    string[] uriParts = uri.Split( "/" );
	    
	    if ( uri.Contains( "watch" ) )
	    {
		    string id = uriParts.Last( ).Split( '=' )[1].Split( "&" ).First();
		    
		    var videoRequest = _youtubeService.Videos.List( "snippet" );
		    videoRequest.Id = id;
		    videoRequest.MaxResults = 1;
        var resp = videoRequest.Execute( );
        if ( resp == null || !resp.Items.Any( ) )
        {
          return null;
        }
        var ytTrack = resp.Items.First( ).Snippet;
        
		    return new Track( ytTrack.Title, 
													ytTrack.ChannelTitle.Split( '-' ).First(), 
													ytTrack.Tags.Count > 1? ytTrack.Tags[^2]: ytTrack.Tags[0],
													0 );
	    }
	    
	    if (uri.Contains( "playlist" ))
	    {
		    string id = uriParts.Last( ).Split( '=' ).Last( );

        var playlistRequest = _youtubeService.Playlists.List( "snippet" );
		    playlistRequest.Id = id;
		    playlistRequest.MaxResults = 1;
		    var playlistResp = playlistRequest.Execute(  );
        if ( playlistResp == null || !playlistResp.Items.Any( ) )
        {
          return null;
        }
        var ytAlbum = playlistResp.Items.First( );
		    
		    var playlistItemsRequest = _youtubeService.PlaylistItems.List( "contentDetails" );
		    playlistItemsRequest.PlaylistId = ytAlbum.Id;
		    playlistItemsRequest.MaxResults = 1;
		    var channelResp = playlistItemsRequest.Execute( ); 
        if ( channelResp == null || !channelResp.Items.Any( ) )
        {
          return null;
        }
        var videoId = channelResp.Items.First( ).ContentDetails.VideoId; 
		    
		    var videoRequest = _youtubeService.Videos.List( "snippet" );
		    videoRequest.Id = videoId;
		    videoRequest.MaxResults = 1;
		    var videoResp = videoRequest.Execute(  );
        if ( videoResp == null || !videoResp.Items.Any( ) )
        {
          return null;
        }
		    var ytTrack = videoResp.Items.First( ).Snippet;
        
		    return new Album( ytAlbum.Snippet.Title.Split( '-' ).Last(), 
													ytTrack.ChannelTitle.Split( '-' ).First(),
													"" );
      }
      
	    if (uri.Contains( "channel" ))
	    {
		    var id = uriParts.Last( );

		    var channelRequest = _youtubeService.Channels.List( "snippet" );
		    channelRequest.Id = id;
		    channelRequest.MaxResults = 1;
		    var channelResp = channelRequest.Execute( );
        if ( channelResp == null || !channelResp.Items.Any( ) )
        {
          return null;
        }
		    var ytArtist = channelResp.Items.First( );

		    return new Artist( ytArtist.Snippet.Title.Split( "-" ).First(), 
													 null );
      }
      return null;
    }

		public string SearchMusic( IMusic musicToSearch )
    {
      /*var searchListRequest = _youtubeService.Search.List("snippet");
      searchListRequest.Q = "Seven nation army"; // Replace with your search term.
      searchListRequest.MaxResults = 50;
      var searchListResponse = searchListRequest.Execute(  );*/
      
      return GetSearchUri( musicToSearch );
    }

    public string GetSearchUri( IMusic toSearch )
    {
	    return string.Concat( "https://music.youtube.com/search?q=",
	                          string.Join( '+', toSearch.QueryString.Split( ' ', '’', '\'' ) ) );
    }
  }
}
