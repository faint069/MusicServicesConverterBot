﻿using System;
using System.IO;
using System.Threading;
using Serilog;
using Serilog.Events;

namespace ConverterBot
{
  static class Program
  {
    private static void Main( )
    {
      string logDir = $"{Directory.GetCurrentDirectory( )}{Path.DirectorySeparatorChar}logs";
      if ( !Directory.Exists( logDir ) )
      {
        Directory.CreateDirectory( logDir );
      }
      
      Log.Logger = new LoggerConfiguration( )
        .MinimumLevel.Debug( )
        .MinimumLevel.Override( "Microsoft", LogEventLevel.Warning )
        .MinimumLevel.Override( "SpotifyAPI.Web", Config.LogLevel )
        .MinimumLevel.Override( "Telegram.Bot", Config.LogLevel )
        .MinimumLevel.Override( "Yandex.Music.Api", LogEventLevel.Information )
        .Enrich.FromLogContext( )
        .WriteTo.File( $"{logDir}{Path.DirectorySeparatorChar}{DateTime.Now:yyyy-MM-dd-hh_mm_ss}.log",
          LogEventLevel.Debug )
        .WriteTo.Console( LogEventLevel.Debug )
        .CreateLogger( );
      
      Bot.Bot.StartReceiving(  );

      while ( true )
      {
        Thread.Sleep( 10000 );
      }
    }
  }
}