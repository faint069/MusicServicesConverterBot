﻿using Serilog;
using SpotifyAPI.Web;
using Yandex.Music.Api;
using Yandex.Music.Api.Common;

namespace ConverterBot
{
    public static class Clients
    {
        private static readonly SpotifyClient  _spotify_client;
        private static readonly YandexMusicApi _yandex_music_client;
        private static          AuthStorage    _yAusthStorage;

        static Clients( )
        {
            Log.Information( "Подключение к Spotify..." );

            SpotifyClientConfig spotify_config = SpotifyClientConfig.CreateDefault( )
                                                                    .WithAuthenticator(
                                                                        new ClientCredentialsAuthenticator(
                                                                            Config.SotifyClientID,
                                                                            Config.SotifyClientSecret ) );
            _spotify_client = new SpotifyClient( spotify_config );

            Log.Information( "Подключение к Spotify успешно" );

            Log.Information( "Подключение к Yandex Music..." );

            _yAusthStorage = new AuthStorage(  );
            _yandex_music_client = new YandexMusicApi( );
            _yandex_music_client.User.Authorize( _yAusthStorage, Config.YMLogin, Config.YMPassword );
            
            Log.Information( "Подключение к Yandex Music успешно" );
        }

        public static SpotifyClient  SpotifyClient     => _spotify_client;

        public static YandexMusicApi YandexMusicClient => _yandex_music_client;

        public static AuthStorage    YAusthStorage     => _yAusthStorage;
    }
    
}