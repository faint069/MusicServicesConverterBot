using ConverterBot.Models.Music;
using Google.Apis.Services;
using Google.Apis.Util;
using Google.Apis.YouTube.v3;

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
      _youtubeService = new YouTubeService(new BaseClientService.Initializer()
      {
        ApiKey = Config.YoutubeApiKey,
        ApplicationName = "MusicServicesConverterBot"
      });
      
      
      // Call the search.list method to retrieve results matching the specified query term.

    }

		public IMusic ParseUri( string uri )
    {

      var videoRequest = _youtubeService.Videos.List( "snippet" );
      videoRequest.Id = "w9g6RlFyqUc";
      videoRequest.MaxResults = 50;
      var videoListResponse = videoRequest.Execute(  );
      return null;
    }

		public string SearchMusic( IMusic musicToSearch )
    {
      var searchListRequest = _youtubeService.Search.List("snippet");
      searchListRequest.Q = "Seven nation army"; // Replace with your search term.
      searchListRequest.MaxResults = 50;
      var searchListResponse = searchListRequest.Execute(  );
      
      return GetSearchUri( musicToSearch );
    }

    public string GetSearchUri( IMusic toSearch )
    {
      return string.Concat( "https://music.youtube.com/search?q=",
                            toSearch.QueryString(  ).Replace( ' ', '+' ) );
      
    }
  }
}
