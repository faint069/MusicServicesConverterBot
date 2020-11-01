#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Text.RegularExpressions;
using System.Threading;
using ConverterBot.Builders;
using ConverterBot.Misc;
using ConverterBot.Models;
using ConverterBot.Parsers;
using Serilog;
using Serilog.Events;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace ConverterBot
{
    static class Program
    {
        private static Dictionary<string, IParser> _parsers = null!;
        private static Dictionary<string, IBuilder> _builders = null!;
        
        public static TelegramBotClient MyBbot;
        
        private static void Main( )
        {
            if ( !Directory.Exists( Config.LogPath ) )
            {
                Directory.CreateDirectory( Config.LogPath );
            }

            Log.Logger = new LoggerConfiguration( )
                .MinimumLevel.Debug( )
                .MinimumLevel.Override( "Microsoft", LogEventLevel.Warning )
                .MinimumLevel.Override( "SpotifyAPI.Web", Config.LogLevel )
                .MinimumLevel.Override( "Telegram.Bot", Config.LogLevel )
                .MinimumLevel.Override( "Yandex.Music.Api", LogEventLevel.Information )
                .Enrich.FromLogContext( )
                .WriteTo.File( $"{Config.LogPath}{DateTime.Now:yyyy-MM-dd-hh_mm_ss}.log",
                    LogEventLevel.Debug )
                .WriteTo.Console( LogEventLevel.Debug )
                .CreateLogger( );
            
            Bot.Client.StartReceiving(  );

            while ( true )
            {
                Thread.Sleep( 10000 );
            }
        }
    }
}
