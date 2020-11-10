using ConverterBot.Models;
using ConverterBot.Models.Music;

namespace ConverterBot.Builders
{
  public interface IBuilder
  {
    /// <summary>
    /// Вщзвращает Uri на указанную музыку в своем сервисе
    /// </summary>
    /// <param name="musicToSearch">Музыка которую нужно искать</param>
    /// <returns></returns>
    public string SearchMusic( IMusic musicToSearch );
  }
}