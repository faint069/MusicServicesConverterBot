using ConverterBot.Models;
using ConverterBot.Models.Music;

namespace ConverterBot.Builders
{ 
  /// <summary>
  /// While Youtube Music doesn't have API this builder just returns URI to Youtube Music search page 
  /// </summary>
  public class YoutubeMusicBuilder : IBuilder
  {
    public string SearchMusic( IMusic musicToSearch )
    {
      return "https://music.youtube.com/search?q=" + 
             musicToSearch.QueryString(  ).Replace( ' ', '+' );
    }
  }
}