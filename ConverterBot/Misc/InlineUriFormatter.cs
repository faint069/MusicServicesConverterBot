using System.Collections.Generic;
using ConverterBot.Models.Music;

namespace ConverterBot.Misc
{
  /// <summary>
  /// Formats incoming music with URI into inline format via MarckuoV2
  /// </summary>
  public static class InlineUriFormatter
  {
    public static string FormatTracks( List<(Track, string)> input )
    {
      string output = string.Empty;
      int i = 1;
      foreach ( var item in input )
      {
        output += $"{i}. {item.Item1.Artist} - {item.Item1.Album} - [{item.Item1.Title}]({item.Item2})\n";
        i++;
      }

      return output;
    }
    
    public static string FormatAlbums(List<(Album, string)> input )
    {
      string output = string.Empty;
      int i = 1;
      foreach ( var item in input )
      {
        output += $"{i}. {item.Item1.Artist} - [{item.Item1.Title}]({item.Item2})\n";
        i++;
      }

      return output;
    }
    
    public static string FormatArtists(List<(Artist, string)> input )
    {
      string output = string.Empty;
      int i = 1;
      foreach ( var item in input )
      {
        output += $"{i}. [{item.Item1.Name}]({item.Item2})\n";
        i++;
      }

      return output;
    }
  }
}