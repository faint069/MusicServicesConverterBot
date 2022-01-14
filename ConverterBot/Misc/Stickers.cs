using System;
using System.Collections.Generic;
using System.Linq;

namespace ConverterBot.Misc
{
  static class Stickers
  {
    private static readonly List<string> _stickers = Config.SmockingBotStickers;
    
    public static string GetRandomSmockingBotSticker( )
    {
      return Count != 0 ? Config.SmockingBotStickers.OrderBy( _ => Guid.NewGuid( ) ).FirstOrDefault( ) : null;
    }

    public static int Count => _stickers?.Count ?? 0;
  }
}