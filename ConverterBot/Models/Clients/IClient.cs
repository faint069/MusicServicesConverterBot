using ConverterBot.Models.Music;

namespace ConverterBot.Models.Clients
{
	public interface IClient
	{
		public string FriendlyName { get; }
		public string Name { get; }

		public IMusic ParseUri( string Uri );

		public string SearchMusic( IMusic musicToSearch );
	}
}