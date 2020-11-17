using ConverterBot.Models.Music;

namespace ConverterBot.Models.Clients
{
	public class YoutubeClient : IClient
	{
		private const string friendlyName = "YouTube Music";
		private const string name = "music.youtube";

		public string FriendlyName => friendlyName;

		public string Name => name;

    public YoutubeClient( )
    {
    }

		public IMusic ParseUri( string uri )
    {
      return null;
    }

		public string SearchMusic( IMusic musicToSearch )
    {
      return GetSearchUri( musicToSearch );
    }

    public string GetSearchUri( IMusic toSearch )
    {
      return string.Concat( "https://music.youtube.com/search?q=",
                            toSearch.QueryString(  ).Replace( ' ', '+' ) );
      
    }
  }
}