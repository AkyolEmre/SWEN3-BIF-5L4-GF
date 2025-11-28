using System.Threading.Tasks;

namespace DMS.API.Services
{
    public interface IMessageProducer
    {
        Task SendMessageAsync(string message);
    }
}