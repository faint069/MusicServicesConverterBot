using System;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
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
      PrintCollection( collection );
      return collection.FindOne( _ => _.ChatId == chatId ).Services;
    }

    public static void SetServicesForChat( long chatId, string[] services )
    {
      var collection = Database.GetCollection<ServicesInChat>( "ServicesInChats" );
      PrintCollection( collection );
      if ( collection.Exists( _ => _.ChatId == chatId ) )
      {
        collection.DeleteMany( _ => _.ChatId == chatId );
      }
            
      collection.Insert( new ServicesInChat( chatId, services ) );
    }

    private static void PrintCollection( ILiteCollection<ServicesInChat> collection )
    {
      var col = collection.FindAll( );
      foreach ( var services in collection.FindAll(  ) )
      {
        Console.WriteLine($@"В БД для чата {services.ChatId} Хранятся сервисы: {services.Services[0]} {services.Services[1]}");
      }
    }
  }
}