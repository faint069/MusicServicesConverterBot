using ConverterBot.Models;
using LiteDB;

namespace ConverterBot
{
    public static class DB
    {
        private static LiteDatabase Database;

        static DB( )
        {
            Database = new LiteDatabase( @"mscb.db" );
        }

        public static string[] GetServicesForChat( long chatId )
        {
            var collection = Database.GetCollection<ServicesInChat>( "ServicesInChats" );
            return collection.FindOne( _ => _.ChatId == chatId ).Services;
        }

        public static void SetServicesForChat( long chatId, string[] services )
        {
            var collection = Database.GetCollection<ServicesInChat>( "ServicesInChats" );
            collection.Insert( new ServicesInChat( chatId, services ) );
        }
    }
}