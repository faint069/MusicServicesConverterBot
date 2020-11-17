namespace ConverterBot.Localization
{
  public static class Messages
  {
    public static LocalizedString Greetings         => new LocalizedString( "HelloMessage" );
    public static LocalizedString NoServicesInChat  => new LocalizedString( "NotFoundServicesNessage" );
    public static LocalizedString ServicesInChat    => new LocalizedString( "ServicesInChatMessage" );
    public static LocalizedString SetServicesFirst  => new LocalizedString( "SetServicesFirstMessage" );
    public static LocalizedString SetServicesSecond => new LocalizedString( "SetServicesSecondMessage" );
    public static LocalizedString SetServicesThird  => new LocalizedString( "SetServicesThirdMessage" );
    public static LocalizedString MusicNotFound     => new LocalizedString( "MusicNotFoundMessage" );
    public static LocalizedString CantProcess       => new LocalizedString( "CantProcessMessage" );
    public static LocalizedString Help              => new LocalizedString( "HelpMessage" );
    public static LocalizedString ServiceINonInChat => new LocalizedString( "ServiceIsNotInChatMessage" );
    public static LocalizedString WrongUri          => new LocalizedString( "WrongUriMessage" );
  }
}