using ConverterBot.Models.Music;

namespace ConverterBot.Models.Clients
{
	public interface IClient
	{
		public string FriendlyName { get; }
		public string Name { get; }

		public IMusic ParseUri( string uri );

		public string SearchMusic( IMusic musicToSearch );

    public string GetSearchUri( IMusic toSearch );
  }
}