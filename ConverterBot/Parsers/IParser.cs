using ConverterBot.Models;
using ConverterBot.Models.Music;

namespace ConverterBot.Parsers
{
  public interface IParser
  {
    public IMusic ParseUri( string Uri );
  }
}