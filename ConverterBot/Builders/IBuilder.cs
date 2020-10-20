using ConverterBot.Models;

namespace ConverterBot.Builders
{
    public interface IBuilder
    {
        public string SearchMusic( IMusic musicToSearch );
    }
}