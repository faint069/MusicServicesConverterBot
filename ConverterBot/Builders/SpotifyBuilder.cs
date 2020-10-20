using ConverterBot.Models;

namespace ConverterBot.Builders
{
    public class SpotifyBuilder : IBuilder
    {
        public string SearchMusic( IMusic musicToSearch )
        {
            if ( musicToSearch is Track )
            {
                
            }
            if ( musicToSearch is Album )
            {
                
            }
            if ( musicToSearch is Artist )
            {
                
            }

            return null;
        }
    }
}