namespace ConverterBot.Misc
{
  public static class StringExtensions
  {
    private static string RemoveBrackets( string input )
    {
      if ( !( input.Contains( '(' ) && input.Contains( ')' ) ) )
      {
        return input;
      }

      string result = input;
      if ( input.IndexOf( '(' ) > input.IndexOf( ')' ) )
      {
        result = input.Remove( input.IndexOf( ')' ),
        input.IndexOf( '(' ) - input.IndexOf( ')' ) + 1 );
      }
      
      if ( input.IndexOf( '(' ) < input.IndexOf( ')' ) )
      {
        result = input.Remove( input.IndexOf( '(' ),
        input.IndexOf( ')' ) - input.IndexOf( '(' ) + 1 );
      }
      
      return result.Trim( );
    }

    /// <summary>
    /// Удалаяет из строки круглые скобки и все что находится между ними
    /// </summary>
    /// <param name="input">Входная строка</param>
    /// <returns></returns>
    public static string RemoveAllBrackets( this string input )
    {
      while ( input != RemoveBrackets( input ) )
      {
        input = RemoveBrackets( input );
      }

      return input;
    }
  }
}