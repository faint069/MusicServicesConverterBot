using ConverterBot.Models;

namespace ConverterBot.Parsers
{
    public interface IParser
    {
        public IMusic ParseUri( string Uri );
    }
}