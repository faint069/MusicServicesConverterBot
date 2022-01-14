using System;
using System.Collections.Generic;
using ConverterBot.Models.Music;

namespace ConverterBot.Misc
{
  /// <summary>
  /// Formats incoming music with URI into inline format via MarckupV2
  /// </summary>
  public static class InlineUriFormatter
  {
    public static string FormatTracks( List<(Track, string)> input )
    {
      string output = string.Empty;
      int i = 1;
      foreach ( var (track, uri) in input )
      {
        output += output == string.Empty ? string.Empty : Environment.NewLine;
        
        output += $"0{i}. {track.Artist} - {track.Album} - [{track.Title}]({uri})";
        i++;
      }

      return output;
    }
    
    public static string FormatAlbums(List<(Album, string)> input )
    {
      string output = string.Empty;
      int i = 1;
      foreach ( var (album, uri) in input )
      {
        output += output == string.Empty ? string.Empty : Environment.NewLine;
        
        output += $"{i}. {album.Artist} - [{album.Title}]({uri})";
        i++;
      }

      return output;
    }
    
    public static string FormatArtists(List<(Artist, string)> input )
    {
      string output = string.Empty;
      int i = 1;
      foreach ( var (artist, uri) in input )
      {
        output += output == string.Empty ? string.Empty : Environment.NewLine;
        
        output += $"{i}. [{artist.Name}]({uri})";
        i++;
      }

      return output;
    }
  }
}