using System;
using System.Linq;

namespace ConverterBot.Misc
{
     static class Stickers
    {
        public static string GetRandomSmickingBotSticker( )
        {
            return Config.SmockingBotStickers.OrderBy( x => Guid.NewGuid( ) ).FirstOrDefault( );
        }
    }
}