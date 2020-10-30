using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Serilog.Events;

namespace ConverterBot
{
    public static class Config
    {
        private static readonly LogEventLevel       _log_level;
        private static readonly string              _log_path;
          
        private static readonly string              _spotify_client_id;
        private static readonly string              _spotify_client_secret;
        private static readonly string              _ym_login;
        private static readonly string              _ym_password;
        private static readonly string              _telegram_token;
        private static readonly List<string>        _smocking_bot_stickers;

        static Config( )
        {
            IConfigurationRoot config_builder = new ConfigurationBuilder( )
                                                    .AddJsonFile( "appsettings.json" )
                                                    .Build( );

            _spotify_client_id =     config_builder.GetSection( "Authorization" )
                                                   .GetSection( "Spotify" )
                                                   ["Client_ID"];
            _spotify_client_secret = config_builder.GetSection( "Authorization" )
                                                   .GetSection( "Spotify" )
                                                   ["Client_Secret"];
            _ym_login =              config_builder.GetSection( "Authorization" )
                                                   .GetSection( "YM" )
                                                   ["Login"];
            _ym_password =           config_builder.GetSection( "Authorization" )
                                                   .GetSection( "YM" )
                                                   ["Password"];
            _telegram_token =        config_builder.GetSection( "Authorization" )
                                                   .GetSection( "Telegram" )
                                                   ["Token"];
            _log_level =             ( LogEventLevel ) Enum.Parse( typeof( LogEventLevel ),
                                                   config_builder.GetSection( "Logging" )
                                                   ["Log_Level"] );
            _log_path =              config_builder.GetSection( "Logging" )
                                                   ["Log_Path"];

            _smocking_bot_stickers = config_builder.GetSection( "Stickers:SmokingBot" )
                                                   .Get<List<string>>( );
        }

        public static string        SotifyClientID =>      _spotify_client_id;
        
        public static string        SotifyClientSecret =>  _spotify_client_secret;
        
        public static string        YMLogin =>             _ym_login;
        
        public static string        YMPassword =>          _ym_password;
        
        public static string        TelegramToken =>       _telegram_token;

        public static LogEventLevel LogLevel =>            _log_level;

        public static string        LogPath =>             _log_path;

        public static List<string>  SmockingBotStickers => _smocking_bot_stickers;

    }
}