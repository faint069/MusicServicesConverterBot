using System.Globalization;

namespace ConverterBot.Localization
{
  public class LocalizedString
  {
    public LocalizedString( string messageName )
    {
      CultureInfo ruCulture = new CultureInfo( "ru" );
      CultureInfo enCulture = new CultureInfo( "en" );
      
      _ru = MessagesResources.ResourceManager.GetString( messageName, ruCulture );
      
      _en = MessagesResources.ResourceManager.GetString( messageName, enCulture );
    }

    private readonly string _ru;
    private readonly string _en;

    public string GetLocalized( string culture )
    {
      if ( culture == null || culture.Length < 2)
        return _en;
      
      culture = culture.Substring( 0, 2 );
      switch ( culture )
      {
        case "en":
          return _en;
        case "ru":
          return _ru;
        default:
          return _en;
        
      }
    }
  }
}