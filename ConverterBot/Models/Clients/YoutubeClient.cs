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

		public IMusic ParseUri( string Uri )
		{
			throw new System.NotImplementedException( );
		}

		public string SearchMusic( IMusic musicToSearch )
		{
			return "https://music.youtube.com/search?q=" + 
			       musicToSearch.QueryString(  ).Replace( ' ', '+' );
		}
	}
}