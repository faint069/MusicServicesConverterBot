using System.Collections.Generic;
using System.Linq;
using ConverterBot.Models.Clients;

namespace ConverterBot.Models
{
  public static class Services
  {
    private static readonly List<IClient> Clients = new List<IClient>
    {
      new YandexClient(  ),
      new SpotifyClient(  ),
      new YoutubeClient(  )
    };
    static Services( )
    {
    }

    public static int Count => Clients.Count;

    public static List<string> Names
    {
      get
      {
        return Clients.Select( _ => _.Name ).ToList(  );
      }
    }

    public static List<string> FriendlyNames
    {
      get
      {
        return Clients.Select( _ => _.FriendlyName ).ToList(  );
      }
    }

    public static IClient GetClientFromName( string name )
    {
      return Clients.SingleOrDefault( _ => _.Name == name);
    }
    
    public static IClient GetClientFromFriendlyName( string friendlyName )
    {
      return Clients.SingleOrDefault( _ => _.FriendlyName == friendlyName);
    }

    public static bool TryGetClientForUri( string uri, out IClient client )
    {
      client = Clients.SingleOrDefault( _ => uri.Contains( _.Name ) );
      return client != null;
    }

    public static string GetServiceFriendlyName( string name )
    {
      return Clients.SingleOrDefault( _ => _.Name == name )?.FriendlyName;
    }
  }
}